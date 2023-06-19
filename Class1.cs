using System;
using System.Reflection.Emit;
using AppService = Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;

namespace AutoCADSnowman
{
    public class ShowUI
    {
        [CommandMethod("DisplayUI")]
        public void DisplayUI()
        {
            // Create a form
            Form myForm = new Form();
            myForm.Text = "AutoCAD Interface";
            myForm.Width = 400;
            myForm.Height = 200;

            // Add a label to the form
            var myLabel = new System.Windows.Forms.Label();
            myLabel.Text = "Hello, AutoCAD!";
            myLabel.AutoSize = true;
            myLabel.Location = new System.Drawing.Point(150, 80);
            myForm.Controls.Add(myLabel);

            // Show the form
            AppService.Application.ShowModalDialog(myForm);
        }
    }
    public class ObjectDeletion
    {
        [CommandMethod("DeleteObjectsByColor")]
        public void DeleteObjectsByColor()
        {
            Document doc = AppService.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            // Get the color from the user
            PromptResult colorResult = editor.GetString("Enter the color name or color index to delete objects:");
            if (colorResult.Status != PromptStatus.OK)
                return;

            string colorInput = colorResult.StringResult;
            int colorIndex;
            bool isColorIndex = int.TryParse(colorInput, out colorIndex);

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord modelSpace = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // Iterate through all the entities in model space
                foreach (ObjectId entityId in modelSpace)
                {
                    Entity entity = trans.GetObject(entityId, OpenMode.ForRead) as Entity;
                    if (entity != null)
                    {
                        if (isColorIndex)
                        {
                            // Delete entities with matching color index
                            if (entity.ColorIndex == colorIndex)
                            {
                                entity.UpgradeOpen();
                                entity.Erase();
                            }
                        }
                        else
                        {
                            // Delete entities with matching color name
                            if (entity.Color.ToString().Equals(colorInput, StringComparison.OrdinalIgnoreCase))
                            {
                                entity.UpgradeOpen();
                                entity.Erase();
                            }
                        }
                    }
                }

                trans.Commit();
            }

            editor.WriteMessage("Objects with color " + colorInput + " have been deleted.");
        }
    }
    public class TextObjectListing
    {
        [CommandMethod("ListTextObjects")]
        public void ListTextObjects()
        {
            Document doc = AppService.Application.DocumentManager.MdiActiveDocument;
            Editor editor = doc.Editor;
            Database database = doc.Database;

            using (Transaction transaction = database.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable)transaction.GetObject(database.BlockTableId, OpenMode.ForRead);
                BlockTableRecord modelSpace = (BlockTableRecord)transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead);

                foreach (ObjectId objectId in modelSpace)
                {
                    if (objectId.ObjectClass == RXClass.GetClass(typeof(DBText)))
                    {
                        DBText textObject = (DBText)transaction.GetObject(objectId, OpenMode.ForRead);
                        editor.WriteMessage($"Text Object: {textObject.TextString}\n");
                    }
                }

                transaction.Commit();
            }
        }
    }

    public class SnowmanCreator
    {
        [CommandMethod("CreateSnowman")]
        public void CreateSnowman()
        {
            Document doc = AppService.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor editor = doc.Editor;

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                if (blockTable != null)
                {
                    BlockTableRecord modelSpace = tr.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                    if (modelSpace != null)
                    {
                        Circle baseCircle = new Circle(new Point3d(0, 0, 0), Vector3d.ZAxis, 5);
                        modelSpace.AppendEntity(baseCircle);
                        tr.AddNewlyCreatedDBObject(baseCircle, true);

                        Circle middleCircle = new Circle(new Point3d(0, 0, 5), Vector3d.ZAxis, 3);
                        modelSpace.AppendEntity(middleCircle);
                        tr.AddNewlyCreatedDBObject(middleCircle, true);

                        Circle topCircle = new Circle(new Point3d(0, 0, 9), Vector3d.ZAxis, 2);
                        modelSpace.AppendEntity(topCircle);
                        tr.AddNewlyCreatedDBObject(topCircle, true);

                        Point3d leftEyeCenter = new Point3d(-1, 1, 10);
                        Circle leftEye = new Circle(leftEyeCenter, Vector3d.ZAxis, 0.2);
                        modelSpace.AppendEntity(leftEye);
                        tr.AddNewlyCreatedDBObject(leftEye, true);

                        Point3d rightEyeCenter = new Point3d(1, 1, 10);
                        Circle rightEye = new Circle(rightEyeCenter, Vector3d.ZAxis, 0.2);
                        modelSpace.AppendEntity(rightEye);
                        tr.AddNewlyCreatedDBObject(rightEye, true);

                        Point3d nosePoint = new Point3d(0, 0, 10);
                        DBText nose = new DBText();
                        nose.Position = nosePoint;
                        nose.Height = 0.5;
                        nose.TextString = "*";
                        modelSpace.AppendEntity(nose);
                        tr.AddNewlyCreatedDBObject(nose, true);
                    }
                }

                tr.Commit();
            }
        }
    }
}
