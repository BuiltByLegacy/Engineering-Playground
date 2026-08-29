using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EngineeringPlayground.Flow.Runtime
{
    public enum FlowEditorTool
    {
        Draw,
        Erase,
        Pan
    }

    public sealed class FlowTouchEditor : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IScrollHandler
    {
        [SerializeField] private FlowLabRuntimeController controller;
        [SerializeField] private RectTransform workspace;
        [SerializeField] private FlowEditorTool tool = FlowEditorTool.Draw;
        [SerializeField] private int brushRadius = 1;
        [SerializeField] private int maxUndoSteps = 30;
        [SerializeField] private float minScale = 0.65f;
        [SerializeField] private float maxScale = 2.5f;

        private readonly Stack<bool[]> _undo = new();
        private readonly Stack<bool[]> _redo = new();
        private bool _editing;
        private Vector2Int? _lastCell;

        public void Configure(FlowLabRuntimeController runtimeController, RectTransform workspaceRect)
        {
            controller = runtimeController;
            workspace = workspaceRect;
        }

        private void Awake()
        {
            if (workspace == null)
                workspace = transform as RectTransform;
        }

        public void SetTool(FlowEditorTool value) => tool = value;

        public void Undo()
        {
            if (_undo.Count == 0 || controller?.Solver == null) return;
            _redo.Push(controller.Solver.GetSolidMaskCopy());
            controller.Solver.ApplySolidMask(_undo.Pop());
            controller.ResetSimulation();
        }

        public void Redo()
        {
            if (_redo.Count == 0 || controller?.Solver == null) return;
            PushUndo(controller.Solver.GetSolidMaskCopy());
            controller.Solver.ApplySolidMask(_redo.Pop());
            controller.ResetSimulation();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (controller?.Solver == null) return;
            if (tool == FlowEditorTool.Pan)
            {
                _editing = true;
                return;
            }

            PushUndo(controller.Solver.GetSolidMaskCopy());
            _redo.Clear();
            _editing = true;
            _lastCell = null;
            PaintAt(eventData.position, eventData.pressEventCamera);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_editing || workspace == null) return;
            if (tool == FlowEditorTool.Pan)
            {
                workspace.anchoredPosition += eventData.delta;
                return;
            }
            PaintAt(eventData.position, eventData.pressEventCamera);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_editing) return;
            _editing = false;
            _lastCell = null;
            if (tool != FlowEditorTool.Pan)
                controller.ResetSimulation();
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (workspace == null) return;
            var delta = eventData.scrollDelta.y * 0.08f;
            var next = Mathf.Clamp(workspace.localScale.x + delta, minScale, maxScale);
            workspace.localScale = new Vector3(next, next, 1f);
        }

        private void PaintAt(Vector2 screenPoint, Camera eventCamera)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(workspace, screenPoint, eventCamera, out var local))
                return;

            var rect = workspace.rect;
            var normalizedX = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
            var normalizedY = Mathf.InverseLerp(rect.yMin, rect.yMax, local.y);
            var x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * controller.Solver.Width), 0, controller.Solver.Width - 1);
            var y = Mathf.Clamp(Mathf.FloorToInt(normalizedY * controller.Solver.Height), 0, controller.Solver.Height - 1);
            var current = new Vector2Int(x, y);

            if (_lastCell.HasValue)
                PaintLine(_lastCell.Value, current);
            else
                PaintBrush(current.x, current.y);

            _lastCell = current;
        }

        private void PaintLine(Vector2Int from, Vector2Int to)
        {
            var dx = Mathf.Abs(to.x - from.x);
            var dy = Mathf.Abs(to.y - from.y);
            var steps = Mathf.Max(dx, dy);
            if (steps == 0)
            {
                PaintBrush(to.x, to.y);
                return;
            }

            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                PaintBrush(Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t)), Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t)));
            }
        }

        private void PaintBrush(int centerX, int centerY)
        {
            var makeSolid = tool == FlowEditorTool.Draw;
            for (var y = centerY - brushRadius; y <= centerY + brushRadius; y++)
            {
                for (var x = centerX - brushRadius; x <= centerX + brushRadius; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    if (dx * dx + dy * dy > brushRadius * brushRadius) continue;
                    controller.Solver.SetSolid(x, y, makeSolid);
                }
            }
        }

        private void PushUndo(bool[] mask)
        {
            if (_undo.Count >= maxUndoSteps)
            {
                var temp = _undo.ToArray();
                _undo.Clear();
                for (var i = temp.Length - 2; i >= 0; i--)
                    _undo.Push(temp[i]);
            }
            _undo.Push(mask);
        }
    }
}
