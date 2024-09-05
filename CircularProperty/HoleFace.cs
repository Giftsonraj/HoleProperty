using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NXOpen;
using NXOpen.UF;

namespace CircularProperty
{
    public class HoleFace
    {
        public Session TheSession { get; set; }
        public Part WorkPart { get; set; }
        public ListingWindow ListingUI { get; set; }

        public static void Main()
        {
            HoleFace holeface1 = new HoleFace();
            holeface1.collecting_Data();
        }
        public void collecting_Data()
        {
            TheSession = Session.GetSession();
            UFSession UFSession = UFSession.GetUFSession();
            WorkPart = TheSession.Parts.Work;
            ListingUI = TheSession.ListingWindow;
            ListingUI.Open();

            string executingAssemblyPath =  System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string filePath = Path.Combine(executingAssemblyPath, "HoleType.xml");

            Hole holeData = Hole.ReadFromXml(filePath);
            HashSet<Face> processedFaces = new HashSet<Face>();

            foreach (Body body in WorkPart.Bodies) //Features
            {
                Face[] faces = body.GetFaces();
                foreach (Face bodyFace in faces)
                {
                    if (bodyFace.SolidFaceType != Face.FaceType.Cylindrical)
                    {
                        continue;
                    }
                    if (processedFaces.Contains(bodyFace))
                    {
                        continue;
                    }
                    Edge[] edges = bodyFace.GetEdges();
                    foreach (Edge bodyEdge in edges)
                    {
                        if (bodyEdge.SolidEdgeType == Edge.EdgeType.Circular)
                        {
                            double param = 1.0;
                            double[] point1 = new double[3];
                            double[] tangent1 = new double[3];
                            double[] principalNormal1 = new double[3];
                            double[] binormal1 = new double[3];
                            UFSession.Modl.AskCurveProps(bodyEdge.Tag, param, point1, tangent1, principalNormal1, binormal1, out var _, out var _);
                            double[] endVertex = point1;

                            double param2 = 0.0;
                            double[] point2 = new double[3];
                            double[] tangent2 = new double[3];
                            double[] principalNormal2 = new double[3];
                            double[] binormal2 = new double[3];
                            UFSession.Modl.AskCurveProps(bodyEdge.Tag, param2, point2, tangent2, principalNormal2, binormal2, out var _, out var _);
                            double[] startVertex = point2;


                            //Getting the start & end point with round value
                            if (Math.Round(startVertex[0]) == Math.Round(endVertex[0]) && Math.Round(startVertex[1]) == Math.Round(endVertex[1]) && Math.Round(startVertex[2]) == Math.Round(endVertex[2]))
                            {
                                double edgeLength = bodyEdge.GetLength();
                                double diameter = edgeLength / Math.PI;

                                processedFaces.Add(bodyFace);

                                foreach (var hole in holeData.DowelHoles.Concat(holeData.ClearanceHoles))
                                {
                                    if (Math.Abs(hole.Diameter - diameter) < 0.01) // Tolerance for comparison
                                    {
                                        // Assign attribute to the face
                                        bodyFace.SetAttribute(hole.HoleType, hole.Diameter);
                                    }
                                }
                            }
                            if (processedFaces.Contains(bodyFace))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
        public static int GetUnloadOption()
        {
            return 1;
        }
    }
}
