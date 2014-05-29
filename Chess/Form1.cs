using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.Interaction;
using Coding4Fun.Kinect.WinForm;

namespace Chess
{
    public partial class ChessTable : Form
    {
        // NU Empty (Boş)
        // 1  Pawn (Piyon)
        // 2  Knight (At)
        // 3  Bishop (Fil)
        // 4  Rook (Kale)
        // 5  Queen (Vezir)
        // 6  King (Şah)


        // satır x sütun
        private string[,] CurrentPlacement = new string[8, 8];
        private KinectSensor _sensor;
        private UserInfo[] _userInfo;
        private InteractionStream _interactionStream;
        private Skeleton[] _skeleton;
        public float handX, handY;
        public float handGrippedX, handGrippedY, handReleasedX, handReleasedY;
        public struct PiecePosition
        {
            public int row;
            public int column;

            public PiecePosition(int _row, int _column)
            {
                row = _row;
                column = _column;
            }
        }
        bool isGrippedFirst = false;

        int roundnr = 0;
        static Bitmap Pieces_BackBuffer = new Bitmap(500, 500);
        static Graphics Pieces_BackBufferDC = Graphics.FromImage((Image)Pieces_BackBuffer);
        static string turn = "a";
        InteractionInfo ss = new InteractionInfo();
        public ChessTable()
        {
            InitializeComponent();
            lblTurn.Text = "Turn " + (roundnr+1).ToString() + "\nWaiting for white's movement";
        }

        private void InitChessTable()
        {
            CurrentPlacement[0, 0] = "b4";
            CurrentPlacement[0, 1] = "b2";
            CurrentPlacement[0, 2] = "b3";
            CurrentPlacement[0, 3] = "b5";
            CurrentPlacement[0, 4] = "b6";
            CurrentPlacement[0, 5] = "b3";
            CurrentPlacement[0, 6] = "b2";
            CurrentPlacement[0, 7] = "b4";

            CurrentPlacement[1, 0] = "b1";
            CurrentPlacement[1, 1] = "b1";
            CurrentPlacement[1, 2] = "b1";
            CurrentPlacement[1, 3] = "b1";
            CurrentPlacement[1, 4] = "b1";
            CurrentPlacement[1, 5] = "b1";
            CurrentPlacement[1, 6] = "b1";
            CurrentPlacement[1, 7] = "b1";

            for (int y = 2; y < 6; y++)
                for (int x = 0; x < 8; x++)
                    CurrentPlacement[y, x] = "NU";

            CurrentPlacement[6, 0] = "a1";
            CurrentPlacement[6, 1] = "a1";
            CurrentPlacement[6, 2] = "a1";
            CurrentPlacement[6, 3] = "a1";
            CurrentPlacement[6, 4] = "a1";
            CurrentPlacement[6, 5] = "a1";
            CurrentPlacement[6, 6] = "a1";
            CurrentPlacement[6, 7] = "a1";

            CurrentPlacement[7, 0] = "a4";
            CurrentPlacement[7, 1] = "a2";
            CurrentPlacement[7, 2] = "a3";
            CurrentPlacement[7, 3] = "a5";
            CurrentPlacement[7, 4] = "a6";
            CurrentPlacement[7, 5] = "a3";
            CurrentPlacement[7, 6] = "a2";
            CurrentPlacement[7, 7] = "a4";
        }
        private void ChessTable_Load(object sender, EventArgs e)
        {
            InitChessTable();
            DrawPieces();
            //panelBase.Invalidate(new Rectangle(55, 55, 389, 389));
            if (KinectSensor.KinectSensors.Count() > 0)
            {
                _sensor = KinectSensor.KinectSensors[0];
                if (_sensor.Status == KinectStatus.Connected)
                {
                    _skeleton = new Skeleton[_sensor.SkeletonStream.FrameSkeletonArrayLength];
                    _userInfo = new UserInfo[InteractionFrame.UserInfoArrayLength];


                    _sensor.DepthStream.Range = DepthRange.Near;
                    _sensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);

                    _sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
                    _sensor.SkeletonStream.EnableTrackingInNearRange = true;
                    _sensor.SkeletonStream.Enable(new TransformSmoothParameters()
                        {
                            Smoothing = 0.75f,
                            Correction = 0.07f,
                            Prediction = 0.08f,
                            JitterRadius = 0.08f,
                            MaxDeviationRadius = 0.07f
                        });

                    
                    _interactionStream = new InteractionStream(_sensor, new InteractionClient());
                    _interactionStream.InteractionFrameReady += _interactionStream_InteractionFrameReady;

                    _sensor.DepthFrameReady += _sensor_DepthFrameReady;
                    _sensor.SkeletonFrameReady += _sensor_SkeletonFrameReady;

                    _sensor.Start();
                }
            }
        }

        void _sensor_DepthFrameReady(object sender, DepthImageFrameReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }
                try
                {
                    _interactionStream.ProcessDepth(depthFrame.GetRawPixelData(), depthFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                    
                }
            }
        }


        private Dictionary<int, InteractionHandEventType> _lastRightHandEvents = new Dictionary<int, InteractionHandEventType>();
        private Dictionary<int, InteractionHandEventType> _lastLeftHandEvents = new Dictionary<int, InteractionHandEventType>();

        void _interactionStream_InteractionFrameReady(object sender, InteractionFrameReadyEventArgs e)
        {
            using (var iaf = e.OpenInteractionFrame())
            {
                if (iaf == null)
                {
                    return;
                }
                iaf.CopyInteractionDataTo(_userInfo);
            }

            foreach (var userInfo in _userInfo)
            {
                var userID = userInfo.SkeletonTrackingId;
                if (userID == 0)
                    continue;

                var hands = userInfo.HandPointers;
                if (hands.Count == 0)
                {
                    MessageBox.Show("No hands");
                }
                else
                {
                    foreach (var hand in hands)
                    {
                        var lastHandEvents = hand.HandType == InteractionHandType.Left 
                            ? _lastLeftHandEvents 
                            : _lastRightHandEvents;
                        if (hand.HandEventType == InteractionHandEventType.Grip)
                        {
                            /*handGrippedX = (float) hand.X;
                            handGrippedX = handGrippedX < 0 ? handGrippedX * (-1) + 440 : handGrippedX + 440;
                            handGrippedY = (float) hand.Y;
                            handGrippedY = handGrippedY < 0 ? handGrippedY * (-1) + 440 : handGrippedY + 440;*/
                            
                            kinectDown(handX, handY);
                                                      
                            //MessageBox.Show("tuttu");
                        }
                        if (isGrippedFirst && hand.HandEventType == InteractionHandEventType.GripRelease)
                            {
                                /*handReleasedX = (float) hand.X;
                                handReleasedX = handReleasedX < 0 ? handReleasedX * (-1) + 440 : handReleasedX + 440;
                                handReleasedY = (float) hand.Y;
                                handReleasedY = handReleasedY < 0 ? handReleasedY * (-1) + 440 : handReleasedY + 440;*/

                                kinectUp(handX, handY);
                                //MessageBox.Show("Bıraktı");
                            }
                    }
                }
            }   
            
        }
        
        private void kinectDown(float x, float y)
        {
            if (x > 444 || y > 444 || x < 55 || y < 55)
                return;

            isGrippedFirst = true;
            clickStart = new Point((int)x, (int)y);
        }

        private void kinectUp(float x, float y)
        {
            if (isGrippedFirst)
            {
                isGrippedFirst = false;
                panelBase.Invalidate();

                MovePiece(clickStart, new Point((int)x, (int)y));
            }
        }

        private void kinectMove(float x, float y)
        {
            if (isGrippedFirst)
            {
                if (x > 444 || y > 444 || x < 55 || y < 55)
                    isGrippedFirst = false;

                clickEnd = new Point((int)x, (int)y);
                panelBase.Invalidate();
            }
        }

        void _sensor_SkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null)
                {
                    return;
                }
                try
                {
                    skeletonFrame.CopySkeletonDataTo(_skeleton);
                    var accelorometerReading = _sensor.AccelerometerGetCurrentReading();
                    _interactionStream.ProcessSkeleton(_skeleton, accelorometerReading, skeletonFrame.Timestamp);
                }
                catch (InvalidOperationException)
                {
                }
                /*Skeleton[] skeletondata = new Skeleton[skeletonFrame.SkeletonArrayLength];
                _userInfo = new UserInfo[InteractionFrame.UserInfoArrayLength];

                skeletonFrame.CopySkeletonDataTo(skeletondata);*/

                Skeleton playerSkeleton = (from s in _skeleton where s.TrackingState == SkeletonTrackingState.Tracked select s).FirstOrDefault();
                if (playerSkeleton != null)
                {
                    Joint hand = playerSkeleton.Joints[JointType.HandRight];
                    handX = (hand.Position.X * 2000); //< 0 ? handX * (-1) : handX;
                    handY = (hand.Position.Y * 2000); //< 0 ? handY * (-1) : handY;

                    handX = handX < 0 ? handX * (-1) : handX;
                    handY = handY < 0 ? handY * (-1) : handY;

                    DrawPointer(handX, handY);
                }
            }
        }

        private void DrawPointer(float handX, float handY)
        {

            SolidBrush myBrush = new SolidBrush(Color.Red);
            Graphics g = panelBase.CreateGraphics();
            panelBase.Refresh();
            g.FillEllipse(myBrush, handX, handY, 10, 10);
            myBrush.Dispose();
            g.Dispose();
            kinectMove(handX, handY);
        }

        private void panelBase_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage((Image)Pieces_BackBuffer, new Point(0, 0));

            if (mouseDown && clickEnd != null)
                e.Graphics.DrawLine(vectorPen, clickStart, clickEnd);
            //e.Graphics.DrawEllipse(vectorPen, handX, handY, 5, 5);
        }
        private void DrawPieces()
        {
            Pieces_BackBufferDC.Clear(Color.Transparent);

            Point currentPos = new Point(55, 55);
            Size pieceSize = new Size(49, 49);

            using (Graphics g = Graphics.FromImage((Image)Pieces_BackBuffer))
            {
                for (int y = 0; y < 8; y++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        g.DrawImage(TranslateCodeToPiece(CurrentPlacement[y, x]), new Rectangle(currentPos, pieceSize));
                        currentPos.X += 49;
                    }
                    currentPos.X = 55;
                    currentPos.Y += 49;
                }
            }

            lblPos.Text = String.Empty;
            for (int line = 0; line < 8; line++)
            {
                for (int row = 0; row < 8; row++)
                {
                    lblPos.Text += CurrentPlacement[line, row].Replace("NU", "ee") + " ";
                }
                lblPos.Text += "\n";
            }


        }

        private Image TranslateCodeToPiece(string code)
        {
            if (code == "b1")
                return (Image)(pieces.black_pawn);
            else if (code == "b2")
                return (Image)(pieces.black_knight);
            else if (code == "b3")
                return (Image)(pieces.black_bishop);
            else if (code == "b4")
                return (Image)(pieces.black_rook);
            else if (code == "b5")
                return (Image)(pieces.black_queen);
            else if (code == "b6")
                return (Image)(pieces.black_king);
            else if (code == "a1")
                return (Image)(pieces.white_pawn);
            else if (code == "a2")
                return (Image)(pieces.white_knight);
            else if (code == "a3")
                return (Image)(pieces.white_bishop);
            else if (code == "a4")
                return (Image)(pieces.white_rook);
            else if (code == "a5")
                return (Image)(pieces.white_queen);
            else if (code == "a6")
                return (Image)(pieces.white_king);
            else
                return (Image)(new Bitmap(49, 49));
        }
        private string TranslateLocationToPiece(Point loc)
        {
            int simplifiedY = (loc.Y - 55) / 49;
            int simplifiedX = (loc.X - 55) / 49;

            return CurrentPlacement[simplifiedY, simplifiedX] + ";" + simplifiedY + "x" + simplifiedX;
        }

        private void MovePiece(Point start, Point end)
        {
            if (TranslateLocationToPiece(start) == TranslateLocationToPiece(end))
                return;

            //MessageBox.Show("Starting point: " + start.X + "x" + start.Y + "\nEnding point: " + end.X + "x" + end.Y);
            string temp, id, tableLoc;

            temp = TranslateLocationToPiece(start);

            id = temp.Split(';')[0];
            tableLoc = temp.Split(';')[1];

            string[] newLoc = TranslateLocationToPiece(end).Split(';')[1].Split('x');
            if (id[0] == turn[0])
            {
                if (CanItMoveThere(id, Convert.ToInt32(newLoc[0]) - Convert.ToInt32(tableLoc.Split('x')[0]), Convert.ToInt32(newLoc[1]) - Convert.ToInt32((tableLoc.Split('x')[1])), new PiecePosition(Convert.ToInt32(tableLoc.Split('x')[0]), Convert.ToInt32(tableLoc.Split('x')[1]))))
                {
                    CurrentPlacement[Convert.ToInt32(tableLoc.Split('x')[0]), Convert.ToInt32(tableLoc.Split('x')[1])] = "NU";


                    CurrentPlacement[Convert.ToInt32(newLoc[0]), Convert.ToInt32(newLoc[1])] = id;

                    DrawPieces();
                    panelBase.Invalidate();

                    if (turn == "a")
                    {
                        turn = "b";
                        lblTurn.Text = "Turn " + (roundnr+1).ToString() + "\nWaiting for black's movement";
                        roundnr++;
                    }
                    else
                    {
                        turn = "a";
                        lblTurn.Text = "Turn " + (roundnr+1).ToString() + "\nWaiting for white's movement";
                    }
                        

                    
                }
            }
        }

        private bool CanItMoveThere(string id, int diffY, int diffX, PiecePosition currentpos)
        {
            if (isThereAnAlly(id, new PiecePosition(currentpos.row + diffY, currentpos.column + diffX)) == true)
                return false;

            switch (id[1])
            {
                case '1': //Pawn
                    if ( CrossSectionEnemy(currentpos, id) != String.Empty )
                    {
                        string temporalCheck = CrossSectionEnemy(currentpos, id);


                        if (temporalCheck.Length == 1)
                        {
                            if (temporalCheck[0] == 'l')
                            {
                                if (id[0] == 'a')
                                {
                                    if (diffX == -1 && diffY == -1)
                                        return true;
                                }
                                else
                                {
                                    if (diffX == -1 && diffY == 1)
                                        return true;
                                }
                            }
                            else
                            {
                                if (id[0] == 'a')
                                {
                                    if (diffX == 1 && diffY == -1)
                                        return true;
                                }
                                else
                                {
                                    if (diffX == 1 && diffY == 1)
                                        return true;
                                }
                            }
                        }
                        else if (temporalCheck.Length == 2)
                        {
                            if (id[0] == 'a')
                            {
                                if ((diffX == 1 || diffX == -1) && diffY == -1)
                                    return true;
                            }
                            else
                            {
                                if ((diffX == 1 || diffX == -1) && diffY == 1)
                                    return true;
                            }
                        }
                    }

                    if( !EnemyOnTheWay_Pawn(currentpos,id).Equals(new PiecePosition(-1,-1)) )
                        return false;

                    if (id[0] == 'a')
                    {
                        if (currentpos.row == 6)
                        {
                            if ((diffY == -2 || diffY == -1) && diffX == 0)
                                return true;
                        }
                        else
                        {
                            if (diffY == -1 && diffX == 0)
                                return true;
                        }
                    }
                    else
                    {
                        if (currentpos.row == 1)
                        {
                            if ((diffY == 2 || diffY == 1) && diffX == 0)
                                return true;
                        }
                        else
                        {
                            if (diffY == 1 && diffX == 0)
                                return true;
                        }
                    }
                    break;
                case '2': //Knight
                    if ((Math.Abs(diffX) == 2 && Math.Abs(diffY) == 1) || (Math.Abs(diffY) == 2 && Math.Abs(diffX) == 1))
                        return true;
                    break;
                case '3': //Bishop (Needed)
                    if ( Math.Abs(diffX) == Math.Abs(diffY) && diffX != 0 && diffY != 0 && !isPathBlocked(true, false, currentpos, new PiecePosition(currentpos.row+diffY,currentpos.column+diffX) ))
                        return true;
                    break;
                case '4': //Rook (Needed)
                    if ((diffY != 0 && diffX == 0) || (diffX != 0 && diffY == 0))
                    {
                        if (!isPathBlocked(false, true, currentpos, new PiecePosition(currentpos.row + diffY, currentpos.column + diffX)))
                            return true;
                    }
                    break;
                case '5': //Queen (Needed)
                    if (((Math.Abs(diffX) == Math.Abs(diffY)) || diffX == 0 || diffY == 0) || (diffX != 0 && diffY == 0) || (diffY != 0 && diffX == 0))
                    {
                        if( (diffX == 0 && diffY != 0) || (diffX != 0 && diffY == 0) )
                        {
                            if (!isPathBlocked(false, true, currentpos, new PiecePosition(currentpos.row + diffY, currentpos.column + diffX)))
                                return true;
                        }
                        else if( Math.Abs(diffX) == Math.Abs(diffY) )
                        {
                            if (!isPathBlocked(true, false, currentpos, new PiecePosition(currentpos.row + diffY, currentpos.column + diffX)))
                                return true;
                        }
                        else
                            return false;
                    }
                    break;
                case '6': //King
                    if (Math.Abs(diffX) + Math.Abs(diffY) <= 2 && Math.Abs(diffX) < 2 && Math.Abs(diffY) < 2)
                        return true;
                    break;
                default:
                    break;
            }
            return false;
        }


        private string CrossSectionEnemy(PiecePosition pos, string id)
        {
            string toReturn = String.Empty;

            if (id[0] == 'a')
            {
                if (pos.row - 1 > -1 && pos.column + 1 < 8)
                {
                    if (CurrentPlacement[pos.row - 1, pos.column + 1] != "NU")
                        toReturn += "r";
                }
                if (pos.row - 1 > -1 && pos.column - 1 > -1)
                {
                    if (CurrentPlacement[pos.row - 1, pos.column - 1] != "NU")
                        toReturn += "l";
                }

                return toReturn;
            }
            else
            {
                if (pos.row + 1 < 8 && pos.column + 1 < 8)
                {
                    if (CurrentPlacement[pos.row + 1, pos.column + 1] != "NU")
                        toReturn += "r";
                }
                if (pos.row + 1 < 8 && pos.column - 1 > -1)
                {
                    if (CurrentPlacement[pos.row + 1, pos.column - 1] != "NU")
                        toReturn += "l";
                }

                return toReturn;
            }
        }

        private PiecePosition EnemyOnTheWay_Pawn(PiecePosition pos, string id)
        {
            if (id[0] == 'a')
            {
                if (pos.row - 1 < 0)
                    return new PiecePosition(-1, -1);

                if ( CurrentPlacement[pos.row - 1, pos.column][0] == 'b' )
                    return new PiecePosition(pos.row - 1, pos.column);
            }
            else
            {
                if (pos.row + 1 > 8)
                    return new PiecePosition(-1, -1);

                if (CurrentPlacement[pos.row + 1, pos.column][0] == 'a')
                    return new PiecePosition(pos.row + 1, pos.column);
            }
            return new PiecePosition(-1, -1);
        }

        private bool isThereAnAlly(string id, PiecePosition endpos)
        {
            if (CurrentPlacement[endpos.row, endpos.column][0] == id[0])
                return true;
            else
                return false;
        }

        private bool isPathBlocked(bool cross, bool linear, PiecePosition currentpos, PiecePosition endpos)
        {
            bool toReturn = false;
            if (linear && !toReturn)
            {
                if (endpos.row == currentpos.row)
                {
                    if (endpos.column - currentpos.column > 0)
                    {
                        for (int x = currentpos.column + 1; x < endpos.column; x++)
                        {
                            if (CurrentPlacement[currentpos.row, x] != "NU")
                            {
                                toReturn = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int x = currentpos.column - 1; x > endpos.column; x--)
                        {
                            if (CurrentPlacement[currentpos.row, x] != "NU")
                            {
                                toReturn = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    if(endpos.row - currentpos.row > 0)
                    {
                        for (int y = currentpos.row+1; y < endpos.row; y++)
                        {
                            if (CurrentPlacement[y, currentpos.column] != "NU")
                            {
                                toReturn = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int y = currentpos.row-1; y > endpos.row; y--)
                        {
                            if (CurrentPlacement[y, currentpos.column] != "NU")
                            {
                                toReturn = true;
                                break;
                            }
                        }
                    }
                }
            }
            if (cross && !toReturn)
            {
                if ( Math.Abs(endpos.row - currentpos.row) == Math.Abs(endpos.column - currentpos.column))
                {
                    if (endpos.row - currentpos.row > 0) // South
                    {
                        if (endpos.column - currentpos.column > 0) // Southeast
                        {
                            for (int rowToExamine = currentpos.row + 1; rowToExamine < endpos.row; rowToExamine++)
                            {
                                if (CurrentPlacement[rowToExamine, currentpos.column + (rowToExamine - currentpos.row)] != "NU")
                                {
                                    toReturn = true;
                                    break;
                                }
                            }
                        }
                        else // Southwest
                        {
                            for (int rowToExamine = currentpos.row + 1; rowToExamine < endpos.row; rowToExamine++)
                            {
                                if(CurrentPlacement[rowToExamine, currentpos.column - (rowToExamine - currentpos.row)] != "NU")
                                {
                                    toReturn = true;
                                    break;
                                }
                            }
                        }
                    }
                    else if (endpos.row - currentpos.row < 0) // North
                    {
                        if (endpos.column - currentpos.column > 0) // Northeast
                        {
                            for (int rowToExamine = currentpos.row - 1; rowToExamine > endpos.row; rowToExamine--)
                            {
                                if (CurrentPlacement[rowToExamine, currentpos.column + (currentpos.row - rowToExamine)] != "NU")
                                {
                                    toReturn = true;
                                    break;
                                }
                            }
                        }
                        else // Northwest
                        {
                            for (int rowToExamine = currentpos.row - 1; rowToExamine > endpos.row; rowToExamine--)
                            {
                                if (CurrentPlacement[rowToExamine, currentpos.column - (currentpos.row - rowToExamine)] != "NU")
                                {
                                    toReturn = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                else 
                    return false;
            }
            return toReturn;
        }

        #region Mouse Events
        private bool mouseDown = false;
        private Point clickStart, clickEnd;
        private void panelBase_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.X > 444 || e.Y > 444 || e.X < 55 || e.Y < 55)
                return;

            mouseDown = true;
            clickStart = new Point(e.X, e.Y);
        }

        private void panelBase_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseDown = false;
                panelBase.Invalidate();

                MovePiece(clickStart, new Point(e.X, e.Y));
            }
        }

        Pen vectorPen = new Pen(Color.Crimson, 6);
        private void panelBase_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                if (e.X > 444 || e.Y > 444 || e.X < 55 || e.Y < 55)
                    mouseDown = false;

                clickEnd = new Point(e.X, e.Y);
                panelBase.Invalidate();
            }
        }

        #endregion

        private void ChessTable_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'w')
            {
                turn = "a";
            }
        }

        private void ChessTable_FormClosing(object sender, FormClosingEventArgs e)
        {
            _sensor.Stop();
        }

    }
}
