using System;
using System.Collections.Generic;
using System.Text;

namespace Interlace.Geo
{
    public abstract class DouglasPeuckerBase
    {
		protected abstract void HandleSegment(int i, int j);

        protected abstract double GetMetricDistance(Position a, Position b);
		
		private double GetMetricDistance(Position a, Position b, Position p)
		{
			Position ab = b - a;
			Position ap = p - a;
			double abLength = ab.SquaredDistanceFromOrigin();
			
            Position closest;

			// Handle a zero length segment; where a and b coincide:
			if (abLength == 0.0) 
            {
                closest = a;
            }
            else
            {
    			double projAbAp = Position.DotProduct(ab, ap) / abLength;
    			
    			if (projAbAp <= 0.0) 
                {
                    closest = a;
    			} 
                else if (projAbAp >= 1.0) 
                {
                    closest = b;
    			}
                else
                {
        			closest = new Position(a.X + ab.X * projAbAp, a.Y + ab.Y * projAbAp);
                }
            }

			return GetMetricDistance(closest, p);
		}
		
		private void FindSplit(Polyline l, int i, int j, out double maxDistance, out int maxI)
		{
			maxI = -1;
			maxDistance = 0.0;
			
			if (j - i <= 1) return;
			
			Position a = l[i];
			Position b = l[j];
			
			for (int k = i + 1; k < j; k++) 
            {
				double distance = GetMetricDistance(a, b, l[k]);

				if (distance > maxDistance || maxI == -1) 
                {
					maxI = k;
					maxDistance = distance;
				}
			}
		}
		
		private void Recurse(Polyline input, double threshold, int i, int j)
		{
			double maxDistance;
			int maxI;
			
			FindSplit(input, i, j, out maxDistance, out maxI);
			
			if (maxDistance > threshold) 
            {
				Recurse(input, threshold, i, maxI);
				Recurse(input, threshold, maxI, j);
			} 
            else 
            {
				HandleSegment(i, j);
			}
		}
		
		protected void Simplify(Polyline input, double threshold)
		{
			if (input.Length < 2) return;

			Recurse(input, threshold, 0, input.Length - 1);
		}
    }
}
