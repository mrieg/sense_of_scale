using Aardvark.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aardvark.Fitting
{
    public static class PlaneFitting
    {
        #region least squares

        /// <summary>
        /// Least squares plane fitting to a set of points.
        /// </summary>
        public static Plane3d FitPlane3dLeastSquares(this V3d[] points)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            var weights = new double[points.Length].Set(1.0);
            return PerformOneStep(points, weights);
        }
        
        /// <summary>
        /// Least squares plane fitting to a set of points.
        /// </summary>
        public static bool FitPlane3dLeastSquares(this V3d[] points, out Plane3d plane)
        {
            plane = FitPlane3dLeastSquares(points);
            return (!plane.IsInvalid);
        }

        #endregion

        #region weighted least squares

        /// <summary>
        /// Perform weighted least squares plane fitting
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="thres">The algorithm terminates if the change in mean squared error 
        /// is below the given threshold</param>
        /// <param name="mse">Mean squared error</param>
        /// <returns>The found plane</returns>
        public static Plane3d FitPlane3dWeightedLeastSquares(this V3d[] points, int maxIter, double thres, out double mse)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (points.Length <= 0) throw new InvalidOperationException();
            if (maxIter <= 0)throw new InvalidOperationException();

            Plane3d wlsPlane = new Plane3d();
            var weights = new double[points.Length].Set(1.0);
            var sqDists = new double[weights.Length];

            double meanSquaredErrorNew = 0.0;
            double ch = double.MaxValue;

            for (int iter = 0; iter < maxIter && ch > thres; iter++ )
            {
                double meanSquaredErrorCurrent = meanSquaredErrorNew;

                wlsPlane = PerformOneStep(points, weights);

                sqDists.SetByIndex(i => Fun.Square(wlsPlane.Height(points[i])));

                meanSquaredErrorNew = sqDists.InnerProduct(weights,
                                            (s, w) => s * w,
                                            0.0, (s, m) => s + m) / weights.Aggregate((sum, x) => sum += x);

                weights.SetByIndex(i => 1.0 / (sqDists[i] + 1.0));

                ch = Fun.Abs(meanSquaredErrorNew - meanSquaredErrorCurrent);
            };

            mse = meanSquaredErrorNew;

            return wlsPlane;
        }

        /// <summary>
        /// Perform weighted least squares plane fitting
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="maxIter">Maximum number of iterations</param>
        /// <param name="thres">The algorithm terminates if the change in mean squared error 
        /// is below the given threshold</param>
        /// <param name="mse">Mean squared error</param>
        /// <param name="plane">The found plane</param>
        /// <returns>True if a valid plane was found</returns>
        public static bool FitPlane3dWeightedLeastSquares(this V3d[] points, int maxIter, double thres, out double mse, out Plane3d plane)
        {
            plane = FitPlane3dWeightedLeastSquares(points, maxIter, thres, out mse);
            return (!plane.IsInvalid);
        }

        /// <summary>
        /// Perform weighted least squares plane fitting
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="weights">Point weights</param>
        /// <returns>The found plane</returns>
        public static Plane3d FitPlane3dWeightedLeastSquares(this V3d[] points, double[] weights)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (weights == null) throw new ArgumentNullException(nameof(weights));
            if (points.Length != weights.Length) throw new InvalidOperationException();

            return PerformOneStep(points, weights);
        }

        /// <summary>
        /// Perform weighted least squares plane fitting
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="weights">Point weights</param>
        /// <param name="plane">Found plane (out)</param>
        /// <returns>True if a valid plane was found</returns>
        public static bool FitPlane3dWeightedLeastSquares(this V3d[] points, double[] weights, out Plane3d plane)
        {
            plane = FitPlane3dWeightedLeastSquares(points, weights);
            return (!plane.IsInvalid);
        }

        /// <summary>
        /// Perform one step of the weighted least squares plane fitting
        /// </summary>
        /// <param name="points">Data points</param>
        /// <param name="weights">Corresponding point weights</param>
        /// <returns></returns>
        private static Plane3d PerformOneStep(V3d[] points, double[] weights)
        {
            if (points == null) throw new ArgumentNullException(nameof(points));
            if (weights == null) throw new ArgumentNullException(nameof(weights));
            if (points.Length != weights.Length) throw new InvalidOperationException();

            if (points.Length < 3)
                return Plane3d.Invalid;

            if (points.Length == 3)
            {
                var t0 = points[1] - points[0];
                var t1 = points[2] - points[0];
                var n = t0.Cross(t1).Normalized;
                return new Plane3d(n, points[0]);
            }

            V3d c = V3d.Zero;
            for (var i = 0; i < points.Length; ++i)
                c += weights[i] * points[i];
            c /= weights.Sum();


            var data = new double[points.Length * 3];
            for (int i = 0; i < points.Length; ++i)
            {
                data[3 * i + 0] = Fun.Sqrt(weights[i]) * (points[i].X - c.X);
                data[3 * i + 1] = Fun.Sqrt(weights[i]) * (points[i].Y - c.Y);
                data[3 * i + 2] = Fun.Sqrt(weights[i]) * (points[i].Z - c.Z);
            }

            var A = new Matrix<double>(data, new V2i(3, points.Length));
            A.ComputeSingularValues(out Matrix<double> UT, out Matrix<double> VT, out Matrix<double> S, true);
            var normal = new V3d(VT.SubXVectorWindow(2).CopyWindow().Data);

            var plane = new Plane3d(normal.Normalized, c);

            //-- set the normal to be on the same side as the origin
            if (plane.Sign(V3d.OOO) < 0)
                plane.Reverse();

            return plane;
        }

        #endregion

        #region least median of squares

        /// <summary>
        /// Perform least median of squares plane fitting.
        /// </summary>
        /// <param name="points">Data points</param>
        /// <returns>The found plane</returns>
        public static Plane3d FitPlane3dLeastMedianOfSquares(this V3d[] points)
        {
            if (points.Length < 3)
                return Plane3d.Invalid;

            if (points.Length == 3)
                return new Plane3d(points.First(), points.ElementAt(1), points.Last());

            var plane = new Plane3d();

            var rnd = new Random();
            var relativeOutlierRatio = 0.4;
            var probabilityOfSuccess = 0.999;
            var numOfIterations = (int)(Fun.Log(1.0 - probabilityOfSuccess) / Fun.Log(1.0 - (1.0 - relativeOutlierRatio).Pow(3.0)));

            var leastMedianOfSquaredResiduals = double.PositiveInfinity;

            for (int i = 0; i < numOfIterations; i++)
            {
                int p0Index, p1Index, p2Index;
                bool randomPointsSelected;
                do
                {
                    randomPointsSelected = false;
                    p0Index = rnd.Next(points.Length);
                    p1Index = rnd.Next(points.Length);
                    p2Index = rnd.Next(points.Length);

                    if (p0Index != p1Index && p0Index != p2Index && p1Index != p2Index)
                        randomPointsSelected = true;

                } while (!randomPointsSelected);

                var p0 = points[p0Index];
                var p1 = points[p1Index];
                var p2 = points[p2Index];

                // check if points are degenerated -> same position or all along a line -> continue then
                var candidatePlane = new Plane3d(p0, p1, p2);
                if (candidatePlane.Normal.LengthSquared.IsTiny())
                    continue;

                var medianOfSquaredResiduals = points
                    .Where((p, k) => k != p0Index && k != p1Index && k != p2Index)
                    .Select(p => candidatePlane.Height(p).Square())
                    .Median();

                if (medianOfSquaredResiduals < leastMedianOfSquaredResiduals)
                {
                    leastMedianOfSquaredResiduals = medianOfSquaredResiduals;
                    plane = candidatePlane;
                }
            }

            return plane;
        }

        /// <summary>
        /// Least squares plane fitting to a set of points.
        /// </summary>
        public static bool FitPlane3dLeastMedianOfSquares(this V3d[] points, out Plane3d plane)
        {
            plane = FitPlane3dLeastSquares(points);
            return (!plane.IsInvalid);
        }

        #endregion

        #region ransac

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Returns a least-squares fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length.</param>
        /// <param name="minConsensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be considered (absolute value)</param>
        /// <returns>returns the fitted plane</returns>
        public static Plane3d FitPlane3dRansac(this V3d[] points, int iterations, int minConsensus, double epsilon)
        {
            return FitPlane3dRansac(points, iterations, minConsensus, epsilon, out int[] inlier);
        }

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Returns a least-squares fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length.</param>
        /// <param name="minConsensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be considered (absolute value)</param>
        /// <param name="plane">The found plane</param>
        /// <returns>returns true if a valid plane was found</returns>
        public static bool FitPlane3dRansac(this V3d[] points, int iterations, int minConsensus, double epsilon, out Plane3d plane)
        {
            return FitPlane3dRansac(points, iterations, minConsensus, epsilon, out plane, out int[] inlier);
        }

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Returns a least-squares fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length.</param>
        /// <param name="min_consensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be considered (absolute value)</param>
        /// <param name="inliers"> Returns the indices of inlier-points </param>
        /// <returns>returns the fitted plane</returns>
        public static Plane3d FitPlane3dRansac(this V3d[] points, int iterations, int min_consensus, double epsilon,
            out int[] inliers)
        {
            var plane = new Plane3d();

            if (points.Length >= 3)
            {
                //fit a plane with RANSAC
                var ransac = new RobustEstimation<V3d, Plane3d>(3, pp => new Plane3d(pp[0], pp[1], pp[2]), (p, model) => p.GetMinimalDistanceTo(model));
                plane = ransac.SolveWithRansac(points, epsilon, iterations, 0.99, out inliers);
                if (inliers.Length > min_consensus)
                {
                    //optimize with least squares
                    plane = FitPlane3dLeastSquares(inliers.Map(idx => points[idx]));
                    return plane;
                }
            }

            inliers = null;
            plane = Plane3d.Invalid;
            return plane;
        }

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Returns a least-squares fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length.</param>
        /// <param name="min_consensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be considered (absolute value)</param>
        /// <param name="plane">returns the fitted plane</param>
        /// <param name="inliers"> Returns the indices of inlier-points </param>
        /// <returns>returns true if a valid plane was found</returns>
        public static bool FitPlane3dRansac(this V3d[] points, int iterations, int min_consensus, double epsilon,
            out Plane3d plane, out int[] inliers)
        {
            plane = FitPlane3dRansac(points, iterations, min_consensus, epsilon, out inliers);
            return (!plane.IsInvalid);
        }

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Renturns a least-squeres fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length!!!</param>
        /// <param name="min_consensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be cosidered (absolute value)</param>
        /// <param name="inliers"> Returns the inlier-points </param>        
        /// <returns>the fitted plane</returns>
        public static Plane3d FitPlaneRansac(this V3d[] points, int iterations, int min_consensus, double epsilon, out V3d[] inliers)
        {
            if (FitPlane3dRansac(points, iterations, min_consensus, epsilon, out Plane3d plane, out int[] indices))
                inliers = indices.Map(x => points[x]);
            else
                inliers = null;

            return plane;
        }

        /// <summary>
        /// Performs RANSAC consensus plane fitting. 
        /// Renturns a least-squeres fitted plane to the inliers.
        /// </summary>
        /// <param name="points">Point Array - here the most dominant plane will be fitted</param>
        /// <param name="iterations">Number of RANSAC iterations: depends on the expected ratio between inliers and outliers. Independent of point array's length!!!</param>
        /// <param name="min_consensus">Minimum number of inlier points to be in the plane</param>
        /// <param name="epsilon">The +-distance of the points to the plane to be cosidered (absolute value)</param>
        /// <param name="plane">returns the fitted plane</param>
        /// <param name="inliers"> Returns the inlier-points </param>        
        /// <returns>True if a valid plane was found</returns>
        public static bool FitPlane3dRansac(this V3d[] points, int iterations, int min_consensus, double epsilon,
             out Plane3d plane, out V3d[] inliers)
        {
            plane = FitPlaneRansac(points, iterations, min_consensus, epsilon, out inliers);
            return (!plane.IsInvalid);
        }
        
        #endregion

        #region plane fitting

        /// <summary>
        /// Fits a plane to a set of points using the least median of squares method,
        /// and projects the points to 2D plane space.
        /// </summary>
        /// <param name="points">3D input point cloud</param>
        /// <returns>Tuple with the projected 2D points and the corresponding plane.</returns>
        public static (V2d[], Plane3d) ProjectToFittedPlane(V3d[] points)
        {
            var plane = points.FitPlane3dLeastMedianOfSquares();
            var w2p = plane.GetWorldToPlane();

            return (
                new V2d[points.Length].SetByIndex(i => w2p.TransformPos(points[i].GetClosestPointOn(plane)).XY), plane
                );
        }

        /// <summary>
        /// Fits a plane to a set of points using the least median of squares method,
        /// and projects the points to 2D plane space.
        /// </summary>
        /// <param name="positions">3D input point cloud</param>
        /// <param name="normals">input normals</param>
        /// <returns>Tuple with the projected 2D points and the corresponding plane.</returns>
        public static (V2d[], Plane3d) ProjectToFittedPlane(V3d[] positions, V3d[] normals)
        {
            if (positions == null) throw new NotImplementedException();
            if (normals == null) throw new NotImplementedException();

            var plane = positions.FitPlane3dLeastMedianOfSquares();

            // if plane normal does not align well with specified point normals then reverse plane
            if (plane.Normal.ComputeNormalsAlignmentQuality(normals) < 0.0)
            { 
                plane = plane.Reversed;
            }

            var w2p = plane.GetWorldToPlane();
            var positions2d = positions.Map(x => w2p.TransformPos(x.GetClosestPointOn(plane)).XY);

            return (positions2d, plane);
        }

        /// <summary>
        /// Computes how good the given normals are aligned with the reference normal,
        /// where 1.0 means that all normals are identical to the reference normal,
        /// and -1.0 means that all normals are perfectly opposite to the reference normal.
        /// </summary>
        public static double ComputeNormalsAlignmentQuality(this V3d referenceNormal, V3d[] normals)
        {
            var sum = normals.Sum(n => referenceNormal.Dot(n));
            var quality = sum / normals.Length;
            return quality;
        }

        #endregion
    }
}
