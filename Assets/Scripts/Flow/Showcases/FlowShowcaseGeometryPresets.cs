using System;

namespace EngineeringPlayground.Flow.Showcases
{
    public static class FlowShowcaseGeometryPresets
    {
        public const string ResidentialPlumbing = "showcase_residential_plumbing";
        public const string AutomotiveExhaust = "showcase_automotive_exhaust";
        public const string HvacDistribution = "showcase_hvac_distribution";
        public const string ManifoldOptimization = "showcase_manifold_optimization";

        public static readonly string[] AllIds =
        {
            ResidentialPlumbing,
            AutomotiveExhaust,
            HvacDistribution,
            ManifoldOptimization
        };

        public static bool[] Build(string geometryId, int width, int height)
        {
            if (width < 8) throw new ArgumentOutOfRangeException(nameof(width));
            if (height < 8) throw new ArgumentOutOfRangeException(nameof(height));

            var mask = new bool[width * height];
            EnforceChannelEdges(mask, width, height);

            switch (geometryId)
            {
                case ResidentialPlumbing:
                    BuildResidentialPlumbing(mask, width, height);
                    break;
                case AutomotiveExhaust:
                    BuildAutomotiveExhaust(mask, width, height);
                    break;
                case HvacDistribution:
                    BuildHvacDistribution(mask, width, height);
                    break;
                case ManifoldOptimization:
                    BuildManifoldOptimization(mask, width, height);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(geometryId), geometryId, "Unknown showcase geometry preset.");
            }

            EnforceChannelEdges(mask, width, height);
            return mask;
        }

        private static void BuildResidentialPlumbing(bool[] mask, int width, int height)
        {
            // House/fixture packaging proxy: staggered occupied zones force the player
            // to create a smooth route around service-space restrictions.
            FillNormalizedRect(mask, width, height, 0.27, 0.08, 0.42, 0.43);
            FillNormalizedRect(mask, width, height, 0.53, 0.57, 0.69, 0.91);
            FillNormalizedRect(mask, width, height, 0.77, 0.14, 0.84, 0.31);
        }

        private static void BuildAutomotiveExhaust(bool[] mask, int width, int height)
        {
            // Vehicle underbody proxy: floor/chassis and drivetrain/suspension packaging
            // leave a constrained tunnel that rewards smoother routing.
            FillNormalizedRect(mask, width, height, 0.22, 0.69, 0.79, 0.88);
            FillNormalizedRect(mask, width, height, 0.40, 0.08, 0.55, 0.37);
            FillNormalizedRect(mask, width, height, 0.72, 0.10, 0.83, 0.34);
            FillNormalizedRect(mask, width, height, 0.18, 0.10, 0.25, 0.24);
        }

        private static void BuildHvacDistribution(bool[] mask, int width, int height)
        {
            // Symmetric duct/plenum proxy. This is intentionally not a true multi-zone
            // network; the obstacles create a visibly symmetric distribution problem.
            FillNormalizedRect(mask, width, height, 0.40, 0.36, 0.48, 0.64);
            FillNormalizedRect(mask, width, height, 0.64, 0.70, 0.79, 0.86);
            FillNormalizedRect(mask, width, height, 0.64, 0.14, 0.79, 0.30);
            FillNormalizedRect(mask, width, height, 0.82, 0.44, 0.88, 0.56);
        }

        private static void BuildManifoldOptimization(bool[] mask, int width, int height)
        {
            // Four-lane manifold proxy. Three separators form four visible passages,
            // while the solver still uses a single right-side outlet boundary.
            FillNormalizedRect(mask, width, height, 0.35, 0.40, 0.47, 0.60);
            FillNormalizedRect(mask, width, height, 0.58, 0.31, 0.87, 0.345);
            FillNormalizedRect(mask, width, height, 0.58, 0.485, 0.87, 0.52);
            FillNormalizedRect(mask, width, height, 0.58, 0.66, 0.87, 0.695);
        }

        private static void FillNormalizedRect(
            bool[] mask,
            int width,
            int height,
            double minX,
            double minY,
            double maxX,
            double maxY)
        {
            var x0 = Clamp((int)Math.Round(minX * (width - 1)), 2, width - 3);
            var x1 = Clamp((int)Math.Round(maxX * (width - 1)), 2, width - 3);
            var y0 = Clamp((int)Math.Round(minY * (height - 1)), 1, height - 2);
            var y1 = Clamp((int)Math.Round(maxY * (height - 1)), 1, height - 2);

            if (x1 < x0) (x0, x1) = (x1, x0);
            if (y1 < y0) (y0, y1) = (y1, y0);

            for (var y = y0; y <= y1; y++)
            for (var x = x0; x <= x1; x++)
                mask[y * width + x] = true;
        }

        private static void EnforceChannelEdges(bool[] mask, int width, int height)
        {
            for (var x = 0; x < width; x++)
            {
                mask[x] = true;
                mask[(height - 1) * width + x] = true;
            }

            for (var y = 1; y < height - 1; y++)
            {
                mask[y * width + 1] = false;
                mask[y * width + width - 2] = false;
            }
        }

        private static int Clamp(int value, int min, int max) =>
            value < min ? min : value > max ? max : value;
    }
}
