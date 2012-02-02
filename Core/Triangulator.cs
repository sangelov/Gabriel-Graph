using System;
using System.Collections.Generic;
using System.Diagnostics;

/*
  copyright s-hull.org 2011
  released under the contributors beerware license

  contributors: Phil Atkin, Dr Sinclair.
*/
namespace DelaunayTriangulator
{
	public class Triangulator
	{
		private List<Vertex> points;

		public const float Fraction = 0.3f;

		public Triangulator()
		{
		}

		private void Analyse(List<Vertex> suppliedPoints, Hull hull, List<Triad> triads, bool rejectDuplicatePoints, bool hullOnly)
		{
			if (suppliedPoints.Count < 3)
			{
				throw new ArgumentException("Number of points supplied must be >= 3");
			}

			this.points = suppliedPoints;
			int nump = points.Count;

			float[] distance2ToCentre = new float[nump];
			int[] sortedIndices = new int[nump];

			// Choose first point as the seed
			for (int k = 0; k < nump; k++)
			{
				distance2ToCentre[k] = points[0].Distance2To(points[k]);
				sortedIndices[k] = k;
			}

			// Sort by distance to seed point
			Array.Sort(distance2ToCentre, sortedIndices);

			// Duplicates are more efficiently rejected now we have sorted the vertices
			if (rejectDuplicatePoints)
			{
				// Search backwards so each removal is independent of any other
				for (int k = nump - 2; k >= 0; k--)
				{
					// If the points are identical then their distances will be the same,
					// so they will be adjacent in the sorted list
					if ((points[sortedIndices[k]].X == points[sortedIndices[k + 1]].X) &&
						(points[sortedIndices[k]].Y == points[sortedIndices[k + 1]].Y))
					{
						// Duplicates are expected to be rare, so this is not particularly efficient
						Array.Copy(sortedIndices, k + 2, sortedIndices, k + 1, nump - k - 2);
						Array.Copy(distance2ToCentre, k + 2, distance2ToCentre, k + 1, nump - k - 2);
						nump--;
					}
				}
			}

			Debug.WriteLine((points.Count - nump).ToString() + " duplicate points rejected");

			if (nump < 3)
			{
				throw new ArgumentException("Number of unique points supplied must be >= 3");
			}

			int mid = -1;
			float romin2 = float.MaxValue, circumCentreX = 0, circumCentreY = 0;

			// Find the point which, with the first two points, creates the triangle with the smallest circumcircle
			Triad tri = new Triad(sortedIndices[0], sortedIndices[1], 2, points);
			for (int kc = 2; kc < nump; kc++)
			{
				tri.C = sortedIndices[kc];
				if (tri.FindCircumcirclePrecisely(points) && tri.CircumcircleR2 < romin2)
				{
					mid = kc;
					// Centre of the circumcentre of the seed triangle
					romin2 = tri.CircumcircleR2;
					circumCentreX = tri.CircumcircleX;
					circumCentreY = tri.CircumcircleY;
				}
				else if (romin2 * 4 < distance2ToCentre[kc])
				{
					break;
				}
			}

			// Change the indices, if necessary, to make the 2th point produce the smallest circumcircle with the 0th and 1th
			if (mid != 2)
			{
				int indexMid = sortedIndices[mid];
				float distance2Mid = distance2ToCentre[mid];

				Array.Copy(sortedIndices, 2, sortedIndices, 3, mid - 2);
				Array.Copy(distance2ToCentre, 2, distance2ToCentre, 3, mid - 2);
				sortedIndices[2] = indexMid;
				distance2ToCentre[2] = distance2Mid;
			}

			// These three points are our seed triangle
			tri.C = sortedIndices[2];
			tri.MakeClockwise(points);
			tri.FindCircumcirclePrecisely(points);

			// Add tri as the first triad, and the three points to the convex hull
			triads.Add(tri);
			hull.Add(new HullVertex(points, tri.A));
			hull.Add(new HullVertex(points, tri.B));
			hull.Add(new HullVertex(points, tri.C));

			// Sort the remainder according to their distance from its centroid
			// Re-measure the points' distances from the centre of the circumcircle
			Vertex centre = new Vertex(circumCentreX, circumCentreY);
			for (int k = 3; k < nump; k++)
			{
				distance2ToCentre[k] = points[sortedIndices[k]].Distance2To(centre);
			}

			// Sort the _other_ points in order of distance to circumcentre
			Array.Sort(distance2ToCentre, sortedIndices, 3, nump - 3);

			// Add new points into hull (removing obscured ones from the chain)
			// and creating triangles....
			int numt = 0;
			for (int k = 3; k < nump; k++)
			{
				int pointsIndex = sortedIndices[k];
				HullVertex ptx = new HullVertex(points, pointsIndex);

				float dx = ptx.X - hull[0].X, dy = ptx.Y - hull[0].Y;  // outwards pointing from hull[0] to pt.

				int numh = hull.Count;
				List<int> pidx = new List<int>(), tridx = new List<int>();
				int hidx;  // new hull point location within hull.....

				if (hull.EdgeVisibleFrom(0, dx, dy))
				{
					// starting with a visible hull facet !!!
					hidx = 0;

					// check to see if segment numh is also visible
					if (hull.EdgeVisibleFrom(numh - 1, dx, dy))
					{
						// visible.
						pidx.Add(hull[numh - 1].PointsIndex);
						tridx.Add(hull[numh - 1].TriadIndex);

						for (int h = 0; h < numh - 1; h++)
						{
							// if segment h is visible delete h
							pidx.Add(hull[h].PointsIndex);
							tridx.Add(hull[h].TriadIndex);
							if (hull.EdgeVisibleFrom(h, ptx))
							{
								hull.RemoveAt(h);
								h--;
								numh--;
							}
							else
							{
								// quit on invisibility
								hull.Insert(0, ptx);
								numh++;
								break;
							}
						}
						// look backwards through the hull structure
						for (int h = numh - 2; h > 0; h--)
						{
							// if segment h is visible delete h + 1
							if (hull.EdgeVisibleFrom(h, ptx))
							{
								pidx.Insert(0, hull[h].PointsIndex);
								tridx.Insert(0, hull[h].TriadIndex);
								hull.RemoveAt(h + 1);  // erase end of chain
							}
							else
							{
								break; // quit on invisibility
							}// quit on invisibility
						}
					}
					else
					{
						hidx = 1;  // keep pt hull[0]
						tridx.Add(hull[0].TriadIndex);
						pidx.Add(hull[0].PointsIndex);

						for (int h = 1; h < numh; h++)
						{
							// if segment h is visible delete h  
							pidx.Add(hull[h].PointsIndex);
							tridx.Add(hull[h].TriadIndex);
							if (hull.EdgeVisibleFrom(h, ptx))
							{                     // visible
								hull.RemoveAt(h);
								h--;
								numh--;
							}
							else
							{
								// quit on invisibility
								hull.Insert(h, ptx);
								break;
							}
						}
					}
				}
				else
				{
					int e1 = -1, e2 = numh;
					for (int h = 1; h < numh; h++)
					{
						if (hull.EdgeVisibleFrom(h, ptx))
						{
							if (e1 < 0)
							{
								e1 = h;  // first visible
							}// first visible
						}
						else
						{
							if (e1 > 0)
							{
								// first invisible segment.
								e2 = h;
								break;
							}
						}
					}

					// triangle pidx starts at e1 and ends at e2 (inclusive).	
					if (e2 < numh)
					{
						for (int e = e1; e <= e2; e++)
						{
							pidx.Add(hull[e].PointsIndex);
							tridx.Add(hull[e].TriadIndex);
						}
					}
					else
					{
						for (int e = e1; e < e2; e++)
						{
							pidx.Add(hull[e].PointsIndex);
							tridx.Add(hull[e].TriadIndex);   // there are only n-1 triangles from n hull pts.
						}
						pidx.Add(hull[0].PointsIndex);
					}

					// erase elements e1+1 : e2-1 inclusive.
					if (e1 < e2 - 1)
					{
						hull.RemoveRange(e1 + 1, e2 - e1 - 1);
					}

					// insert ptx at location e1+1.
					hull.Insert(e1 + 1, ptx);
					hidx = e1 + 1;
				}

				// If we're only computing the hull, we're done with this point
				if (hullOnly)
				{
					continue;
				}

				int a = pointsIndex, t0;

				int npx = pidx.Count - 1;
				numt = triads.Count;
				t0 = numt;

				for (int p = 0; p < npx; p++)
				{
					Triad trx = new Triad(a, pidx[p], pidx[p + 1], points);
					trx.FindCircumcirclePrecisely(points);

					trx.Bc = tridx[p];
					if (p > 0)
					{
						trx.Ab = numt - 1;
					}
					trx.Ac = numt + 1;

					// index back into the triads.
					Triad txx = triads[tridx[p]];
					if ((trx.B == txx.A && trx.C == txx.B) | (trx.B == txx.B && trx.C == txx.A))
					{
						txx.Ab = numt;
					}
					else if ((trx.B == txx.A && trx.C == txx.C) | (trx.B == txx.C && trx.C == txx.A))
					{
						txx.Ac = numt;
					}
					else if ((trx.B == txx.B && trx.C == txx.C) | (trx.B == txx.C && trx.C == txx.B))
					{
						txx.Bc = numt;
					}

					triads.Add(trx);
					numt++;
				}
				// Last edge is on the outside
				triads[numt - 1].Ac = -1;

				hull[hidx].TriadIndex = numt - 1;
				if (hidx > 0)
				{
					hull[hidx - 1].TriadIndex = t0;
				}
				else
				{
					numh = hull.Count;
					hull[numh - 1].TriadIndex = t0;
				}
			}
		}

		/// <summary>
		/// Return the convex hull of the supplied points,
		/// Don't check for duplicate points
		/// </summary>
		/// <param name="points">List of 2D vertices</param>
		/// <returns></returns>
		public List<Vertex> ConvexHull(List<Vertex> points)
		{
			return ConvexHull(points, false);
		}

		/// <summary>
		/// Return the convex hull of the supplied points,
		/// Optionally check for duplicate points
		/// </summary>
		/// <param name="points">List of 2D vertices</param>
		/// <param name="rejectDuplicatePoints">Whether to omit duplicated points</param>
		/// <returns></returns>
		public List<Vertex> ConvexHull(List<Vertex> points, bool rejectDuplicatePoints)
		{
			Hull hull = new Hull();
			List<Triad> triads = new List<Triad>();

			Analyse(points, hull, triads, rejectDuplicatePoints, true);

			List<Vertex> hullVertices = new List<Vertex>();
			foreach (HullVertex hv in hull)
			{
				hullVertices.Add(new Vertex(hv.X, hv.Y));
			}

			return hullVertices;
		}

		/// <summary>
		/// Return the Delaunay triangulation of the supplied points
		/// Don't check for duplicate points
		/// </summary>
		/// <param name="points">List of 2D vertices</param>
		/// <returns>Triads specifying the triangulation</returns>
		public List<Triad> Triangulation(List<Vertex> points)
		{
			return Triangulation(points, false);
		}

		/// <summary>
		/// Return the Delaunay triangulation of the supplied points
		/// Optionally check for duplicate points
		/// </summary>
		/// <param name="points">List of 2D vertices</param>
		/// <param name="rejectDuplicatePoints">Whether to omit duplicated points</param>
		/// <returns></returns>
		public List<Triad> Triangulation(List<Vertex> points, bool rejectDuplicatePoints)
		{
			List<Triad> triads = new List<Triad>();
			Hull hull = new Hull();

			Analyse(points, hull, triads, rejectDuplicatePoints, false);

			// Now, need to flip any pairs of adjacent triangles not satisfying
			// the Delaunay criterion
			int numt = triads.Count;
			bool[] idsA = new bool[numt];
			bool[] idsB = new bool[numt];

			// We maintain a "list" of the triangles we've flipped in order to propogate any
			// consequent changes
			// When the number of changes is large, this is best maintained as a vector of bools
			// When the number becomes small, it's best maintained as a set
			// We switch between these regimes as the number flipped decreases
			//
			// the iteration cycle limit is included to prevent degenerate cases 'oscillating'
			// and the algorithm failing to stop.
			int flipped = FlipTriangles(triads, idsA);

			int iterations = 1;
			while (flipped > (int)(Fraction * (float)numt) && iterations < 1000)
			{
				if ((iterations & 1) == 1)
				{
					flipped = FlipTriangles(triads, idsA, idsB);
				}
				else
				{
					flipped = FlipTriangles(triads, idsB, idsA);
				}

				iterations++;
			}

			Set<int> idSetA = new Set<int>(), idSetB = new Set<int>();
			flipped = FlipTriangles(triads, ((iterations & 1) == 1) ? idsA : idsB, idSetA);

			iterations = 1;
			while (flipped > 0 && iterations < 2000)
			{
				if ((iterations & 1) == 1)
				{
					flipped = FlipTriangles(triads, idSetA, idSetB);
				}
				else
				{
					flipped = FlipTriangles(triads, idSetB, idSetA);
				}

				iterations++;
			}

			return triads;
		}

		/// <summary>
		/// Test the triad against its 3 neighbours and flip it with any neighbour whose opposite point
		/// is inside the circumcircle of the triad
		/// </summary>
		/// <param name="triads">The triads</param>
		/// <param name="triadIndexToTest">The index of the triad to test</param>
		/// <param name="triadIndexFlipped">Index of adjacent triangle it was flipped with (if any)</param>
		/// <returns>true iff the triad was flipped with any of its neighbours</returns>
		private bool FlipTriangle(List<Triad> triads, int triadIndexToTest, out int triadIndexFlipped)
		{
			int oppositeVertex = 0, edge1, edge2, edge3 = 0, edge4 = 0;
			triadIndexFlipped = 0;

			Triad tri = triads[triadIndexToTest];
			// test all 3 neighbours of tri 

			if (tri.Bc >= 0)
			{
				triadIndexFlipped = tri.Bc;
				Triad t2 = triads[triadIndexFlipped];
				// find relative orientation (shared limb).
				t2.FindAdjacency(tri.B, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
				if (tri.InsideCircumcircle(points[oppositeVertex]))
				{  // not valid in the Delaunay sense.
					edge1 = tri.Ab;
					edge2 = tri.Ac;
					if (edge1 != edge3 && edge2 != edge4)
					{
						int tria = tri.A, trib = tri.B, tric = tri.C;
						tri.Initialize(tria, trib, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
						t2.Initialize(tria, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);

						// change knock on triangle labels.
						if (edge3 >= 0)
						{
							triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
						}
						if (edge2 >= 0)
						{
							triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
						}
						return true;
					}
				}
			}


			if (tri.Ab >= 0)
			{
				triadIndexFlipped = tri.Ab;
				Triad t2 = triads[triadIndexFlipped];
				// find relative orientation (shared limb).
				t2.FindAdjacency(tri.A, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
				if (tri.InsideCircumcircle(points[oppositeVertex]))
				{  // not valid in the Delaunay sense.
					edge1 = tri.Ac;
					edge2 = tri.Bc;
					if (edge1 != edge3 && edge2 != edge4)
					{
						int tria = tri.A, trib = tri.B, tric = tri.C;
						tri.Initialize(tric, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
						t2.Initialize(tric, trib, oppositeVertex, edge2, edge4, triadIndexToTest, points);

						// change knock on triangle labels.
						if (edge3 >= 0)
						{
							triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
						}
						if (edge2 >= 0)
						{
							triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
						}
						return true;
					}
				}
			}

			if (tri.Ac >= 0)
			{
				triadIndexFlipped = tri.Ac;
				Triad t2 = triads[triadIndexFlipped];
				// find relative orientation (shared limb).
				t2.FindAdjacency(tri.A, triadIndexToTest, out oppositeVertex, out edge3, out edge4);
				if (tri.InsideCircumcircle(points[oppositeVertex]))
				{  // not valid in the Delaunay sense.
					edge1 = tri.Ab;   // .ac shared limb
					edge2 = tri.Bc;
					if (edge1 != edge3 && edge2 != edge4)
					{
						int tria = tri.A, trib = tri.B, tric = tri.C;
						tri.Initialize(trib, tria, oppositeVertex, edge1, edge3, triadIndexFlipped, points);
						t2.Initialize(trib, tric, oppositeVertex, edge2, edge4, triadIndexToTest, points);

						// change knock on triangle labels.
						if (edge3 >= 0)
						{
							triads[edge3].ChangeAdjacentIndex(triadIndexFlipped, triadIndexToTest);
						}
						if (edge2 >= 0)
						{
							triads[edge2].ChangeAdjacentIndex(triadIndexToTest, triadIndexFlipped);
						}
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Flip triangles that do not satisfy the Delaunay condition
		/// </summary>
		private int FlipTriangles(List<Triad> triads, bool[] idsFlipped)
		{
			int numt = triads.Count;
			Array.Clear(idsFlipped, 0, numt);

			int flipped = 0;
			for (int t = 0; t < numt; t++)
			{
				int t2;
				if (FlipTriangle(triads, t, out t2))
				{
					flipped += 2;
					idsFlipped[t] = true;
					idsFlipped[t2] = true;

				}
			}

			return flipped;
		}

		private int FlipTriangles(List<Triad> triads, bool[] idsToTest, bool[] idsFlipped)
		{
			int numt = triads.Count;
			Array.Clear(idsFlipped, 0, numt);

			int flipped = 0;
			for (int t = 0; t < numt; t++)
			{
				if (idsToTest[t])
				{
					int t2;
					if (FlipTriangle(triads, t, out t2))
					{
						flipped += 2;
						idsFlipped[t] = true;
						idsFlipped[t2] = true;
					}
				}
			}

			return flipped;
		}

		private int FlipTriangles(List<Triad> triads, bool[] idsToTest, Set<int> idsFlipped)
		{
			int numt = triads.Count;
			idsFlipped.Clear();

			int flipped = 0;
			for (int t = 0; t < numt; t++)
			{
				if (idsToTest[t])
				{
					int t2;
					if (FlipTriangle(triads, t, out t2))
					{
						flipped += 2;
						idsFlipped.Add(t);
						idsFlipped.Add(t2);
					}
				}
			}

			return flipped;
		}

		private int FlipTriangles(List<Triad> triads, Set<int> idsToTest, Set<int> idsFlipped)
		{
			int flipped = 0;
			idsFlipped.Clear();

			foreach (int t in idsToTest)
			{
				int t2;
				if (FlipTriangle(triads, t, out t2))
				{
					flipped += 2;
					idsFlipped.Add(t);
					idsFlipped.Add(t2);
				}
			}

			return flipped;
		}
#if DEBUG
		private void VerifyHullContains(Hull hull, int idA, int idB)
		{
			if (
				((hull[0].PointsIndex == idA) && (hull[hull.Count - 1].PointsIndex == idB)) ||
				((hull[0].PointsIndex == idB) && (hull[hull.Count - 1].PointsIndex == idA)))
			{
				return;
			}

			for (int h = 0; h < hull.Count - 1; h++)
			{
				if (hull[h].PointsIndex == idA)
				{
					Debug.Assert(hull[h + 1].PointsIndex == idB);
					return;
				}
				else if (hull[h].PointsIndex == idB)
				{
					Debug.Assert(hull[h + 1].PointsIndex == idA);
					return;
				}
			}

		}

		private void VerifyTriadContains(Triad tri, int nbourTriad, int idA, int idB)
		{
			if (tri.Ab == nbourTriad)
			{
				Debug.Assert(
					((tri.A == idA) && (tri.B == idB)) ||
					((tri.B == idA) && (tri.A == idB)));
			}
			else if (tri.Ac == nbourTriad)
			{
				Debug.Assert(
					((tri.A == idA) && (tri.C == idB)) ||
					((tri.C == idA) && (tri.A == idB)));
			}
			else if (tri.Bc == nbourTriad)
			{
				Debug.Assert(
					((tri.C == idA) && (tri.B == idB)) ||
					((tri.B == idA) && (tri.C == idB)));
			}
			else
			{
				Debug.Assert(false);
			}
		}

		private void VerifyTriads(List<Triad> triads, Hull hull)
		{
			for (int t = 0; t < triads.Count; t++)
			{
				if (t == 17840)
				{
					t = t + 0;
				}

				Triad tri = triads[t];
				if (tri.Ac == -1)
				{
					VerifyHullContains(hull, tri.A, tri.C);
				}
				else
				{
					VerifyTriadContains(triads[tri.Ac], t, tri.A, tri.C);
				}

				if (tri.Ab == -1)
				{
					VerifyHullContains(hull, tri.A, tri.B);
				}
				else
				{
					VerifyTriadContains(triads[tri.Ab], t, tri.A, tri.B);
				}

				if (tri.Bc == -1)
				{
					VerifyHullContains(hull, tri.B, tri.C);
				}
				else
				{
					VerifyTriadContains(triads[tri.Bc], t, tri.B, tri.C);
				}

			}
		}
#endif
	}
}