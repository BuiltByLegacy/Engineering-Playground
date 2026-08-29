using System;

namespace EngineeringPlayground.Flow.Simulation
{
    public sealed class D2Q9LbmSolver
    {
        private const int Q = 9;
        private static readonly int[] Cx = { 0, 1, 0, -1, 0, 1, -1, -1, 1 };
        private static readonly int[] Cy = { 0, 0, 1, 0, -1, 1, 1, -1, -1 };
        private static readonly int[] Opp = { 0, 3, 4, 1, 2, 7, 8, 5, 6 };
        private static readonly double[] W =
        {
            4.0 / 9.0,
            1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0, 1.0 / 9.0,
            1.0 / 36.0, 1.0 / 36.0, 1.0 / 36.0, 1.0 / 36.0
        };

        private readonly double[] _f;
        private readonly double[] _next;
        private readonly bool[] _solid;
        private readonly double[] _rho;
        private readonly double[] _ux;
        private readonly double[] _uy;

        public D2Q9LbmSolver(int width, int height, double relaxationOmega = 1.82, double inletVelocity = 0.065)
        {
            if (width < 8) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 8) throw new ArgumentOutOfRangeException(nameof(height));
            if (!double.IsFinite(relaxationOmega) || relaxationOmega <= 0.0 || relaxationOmega >= 2.0)
                throw new ArgumentOutOfRangeException(nameof(relaxationOmega), "BGK omega must be between 0 and 2.");
            if (!double.IsFinite(inletVelocity) || inletVelocity < 0.0)
                throw new ArgumentOutOfRangeException(nameof(inletVelocity));

            Width = width;
            Height = height;
            RelaxationOmega = relaxationOmega;
            InletVelocity = inletVelocity;

            var cells = width * height;
            _f = new double[cells * Q];
            _next = new double[cells * Q];
            _solid = new bool[cells];
            _rho = new double[cells];
            _ux = new double[cells];
            _uy = new double[cells];

            EnforceChannelEdges();
            Reset();
        }

        public int Width { get; }
        public int Height { get; }
        public double RelaxationOmega { get; }
        public double InletVelocity { get; set; }
        public double LatticeSoundSpeed => 1.0 / Math.Sqrt(3.0);
        public double InletMachNumber => InletVelocity / LatticeSoundSpeed;
        public bool IsLowMach(double maxMach = 0.1) => InletMachNumber <= maxMach;

        public ReadOnlySpan<double> Density => _rho;
        public ReadOnlySpan<double> VelocityX => _ux;
        public ReadOnlySpan<double> VelocityY => _uy;
        public ReadOnlySpan<bool> Solid => _solid;

        public void Reset()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cell(x, y);
                    var ux = _solid[cell] ? 0.0 : InletVelocity;
                    _rho[cell] = 1.0;
                    _ux[cell] = ux;
                    _uy[cell] = 0.0;
                    for (var i = 0; i < Q; i++)
                        _f[Slot(cell, i)] = Equilibrium(i, 1.0, ux, 0.0);
                }
            }

            Array.Copy(_f, _next, _f.Length);
        }

        public void SetSolid(int x, int y, bool solid)
        {
            if ((uint)x >= Width || (uint)y >= Height)
                return;
            _solid[Cell(x, y)] = solid;
            EnforceChannelEdges();
        }

        public bool[] GetSolidMaskCopy()
        {
            var copy = new bool[_solid.Length];
            Array.Copy(_solid, copy, _solid.Length);
            return copy;
        }

        public void ApplySolidMask(bool[] mask, bool reset = true)
        {
            if (mask == null) throw new ArgumentNullException(nameof(mask));
            if (mask.Length != _solid.Length)
                throw new ArgumentException("Solid mask size does not match solver grid.", nameof(mask));

            Array.Copy(mask, _solid, _solid.Length);
            EnforceChannelEdges();
            if (reset) Reset();
        }

        public void ClearInteriorSolids()
        {
            Array.Fill(_solid, false);
            EnforceChannelEdges();
            Reset();
        }

        public void AddCircularObstacle(int centerX, int centerY, int radius)
        {
            if (radius < 1) throw new ArgumentOutOfRangeException(nameof(radius));
            for (var y = 1; y < Height - 1; y++)
            {
                for (var x = 1; x < Width - 1; x++)
                {
                    var dx = x - centerX;
                    var dy = y - centerY;
                    if (dx * dx + dy * dy <= radius * radius)
                        _solid[Cell(x, y)] = true;
                }
            }
            EnforceChannelEdges();
            Reset();
        }

        public void Step(int iterations = 1)
        {
            if (iterations < 1) return;
            for (var i = 0; i < iterations; i++)
            {
                CollideAndStream();
                ApplyInletOutlet();
                ReconstructMacros();
            }
        }

        public double MeanOutletSpeed()
        {
            var sum = 0.0;
            var count = 0;
            for (var y = 1; y < Height - 1; y++)
            {
                var cell = Cell(Width - 2, y);
                if (_solid[cell]) continue;
                sum += Math.Sqrt(_ux[cell] * _ux[cell] + _uy[cell] * _uy[cell]);
                count++;
            }
            return count == 0 ? 0.0 : sum / count;
        }

        public double MeanDensityAtColumn(int x)
        {
            if ((uint)x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            var sum = 0.0;
            var count = 0;
            for (var y = 1; y < Height - 1; y++)
            {
                var cell = Cell(x, y);
                if (_solid[cell]) continue;
                sum += _rho[cell];
                count++;
            }
            return count == 0 ? 0.0 : sum / count;
        }

        public double MassFluxAtColumn(int x)
        {
            if ((uint)x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
            var flux = 0.0;
            for (var y = 1; y < Height - 1; y++)
            {
                var cell = Cell(x, y);
                if (_solid[cell]) continue;
                flux += _rho[cell] * _ux[cell];
            }
            return flux;
        }

        public double RelativeMassFluxError()
        {
            var inlet = Math.Abs(MassFluxAtColumn(1));
            var outlet = Math.Abs(MassFluxAtColumn(Width - 2));
            var denom = Math.Max(1e-12, inlet);
            return Math.Abs(outlet - inlet) / denom;
        }

        public double MeanAbsoluteVorticity()
        {
            var sum = 0.0;
            var count = 0;
            for (var y = 1; y < Height - 1; y++)
            {
                for (var x = 1; x < Width - 1; x++)
                {
                    var c = Cell(x, y);
                    if (_solid[c]) continue;
                    var dvDx = 0.5 * (_uy[Cell(x + 1, y)] - _uy[Cell(x - 1, y)]);
                    var duDy = 0.5 * (_ux[Cell(x, y + 1)] - _ux[Cell(x, y - 1)]);
                    sum += Math.Abs(dvDx - duDy);
                    count++;
                }
            }
            return count == 0 ? 0.0 : sum / count;
        }

        public double VorticityAt(int x, int y)
        {
            if (x <= 0 || x >= Width - 1 || y <= 0 || y >= Height - 1) return 0.0;
            var c = Cell(x, y);
            if (_solid[c]) return 0.0;
            var dvDx = 0.5 * (_uy[Cell(x + 1, y)] - _uy[Cell(x - 1, y)]);
            var duDy = 0.5 * (_ux[Cell(x, y + 1)] - _ux[Cell(x, y - 1)]);
            return dvDx - duDy;
        }

        public bool IsFinite()
        {
            for (var i = 0; i < _rho.Length; i++)
            {
                if (!double.IsFinite(_rho[i]) || !double.IsFinite(_ux[i]) || !double.IsFinite(_uy[i]))
                    return false;
            }
            return true;
        }

        private void EnforceChannelEdges()
        {
            for (var x = 0; x < Width; x++)
            {
                _solid[Cell(x, 0)] = true;
                _solid[Cell(x, Height - 1)] = true;
            }

            for (var y = 1; y < Height - 1; y++)
            {
                _solid[Cell(1, y)] = false;
                _solid[Cell(Width - 2, y)] = false;
            }
        }

        private void CollideAndStream()
        {
            Array.Clear(_next, 0, _next.Length);

            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cell(x, y);
                    if (_solid[cell])
                    {
                        for (var i = 0; i < Q; i++)
                            _next[Slot(cell, Opp[i])] += _f[Slot(cell, i)];
                        continue;
                    }

                    var rho = 0.0;
                    var ux = 0.0;
                    var uy = 0.0;
                    for (var i = 0; i < Q; i++)
                    {
                        var fi = _f[Slot(cell, i)];
                        rho += fi;
                        ux += fi * Cx[i];
                        uy += fi * Cy[i];
                    }

                    if (rho <= 1e-12) rho = 1.0;
                    ux /= rho;
                    uy /= rho;

                    for (var i = 0; i < Q; i++)
                    {
                        var fi = _f[Slot(cell, i)];
                        var feq = Equilibrium(i, rho, ux, uy);
                        var post = fi - RelaxationOmega * (fi - feq);
                        var nx = x + Cx[i];
                        var ny = y + Cy[i];

                        if (nx < 0 || nx >= Width || ny < 0 || ny >= Height || _solid[Cell(nx, ny)])
                            _next[Slot(cell, Opp[i])] += post;
                        else
                            _next[Slot(Cell(nx, ny), i)] += post;
                    }
                }
            }

            Array.Copy(_next, _f, _f.Length);
        }

        private void ApplyInletOutlet()
        {
            for (var y = 1; y < Height - 1; y++)
            {
                var inlet = Cell(1, y);
                for (var i = 0; i < Q; i++)
                    _f[Slot(inlet, i)] = Equilibrium(i, 1.0, InletVelocity, 0.0);

                var outlet = Cell(Width - 2, y);
                var upstream = Cell(Width - 3, y);
                if (_solid[outlet] || _solid[upstream]) continue;
                for (var i = 0; i < Q; i++)
                    _f[Slot(outlet, i)] = _f[Slot(upstream, i)];
            }
        }

        private void ReconstructMacros()
        {
            for (var y = 0; y < Height; y++)
            {
                for (var x = 0; x < Width; x++)
                {
                    var cell = Cell(x, y);
                    if (_solid[cell])
                    {
                        _rho[cell] = 1.0;
                        _ux[cell] = 0.0;
                        _uy[cell] = 0.0;
                        continue;
                    }

                    var rho = 0.0;
                    var ux = 0.0;
                    var uy = 0.0;
                    for (var i = 0; i < Q; i++)
                    {
                        var fi = _f[Slot(cell, i)];
                        rho += fi;
                        ux += fi * Cx[i];
                        uy += fi * Cy[i];
                    }

                    if (rho <= 1e-12) rho = 1.0;
                    _rho[cell] = rho;
                    _ux[cell] = ux / rho;
                    _uy[cell] = uy / rho;
                }
            }
        }

        private static double Equilibrium(int i, double rho, double ux, double uy)
        {
            var cu = 3.0 * (Cx[i] * ux + Cy[i] * uy);
            var u2 = ux * ux + uy * uy;
            return W[i] * rho * (1.0 + cu + 0.5 * cu * cu - 1.5 * u2);
        }

        private int Cell(int x, int y) => y * Width + x;
        private static int Slot(int cell, int direction) => cell * Q + direction;
    }
}
