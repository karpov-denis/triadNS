using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DrawingPanel
{
    /// <summary>
    /// ImageBox 
    /// </summary>
    [Serializable]
    public class ImgBox : BaseObject
    {
        private Bitmap _img;
        private bool _trasparent = false;

        public ImgBox(DrawingPanel panel, int x, int y, int x1, int y1)
            : base(panel)
        {
            this.X = x;
            this.Y = y;
            this.X1 = x1;
            this.Y1 = y1;
            this.ConnectionPoint = new CConnectionPoint(drawingPanel, this);
            this.bSelected = true;
            this.endMoveRedim();
            this.bCanRotate = false;
        }

        public ImgBox(DrawingPanel panel)
            : base(panel)
        {
            this.ConnectionPoint = new CConnectionPoint(drawingPanel, this);
            this.bSelected = true;
            this.bCanRotate = false;
        }

        public Rectangle Rect
        {
            get
            {
                return new Rectangle(X, Y, X1-X, Y1-Y);
            }
            set
            {
                this.X = value.Left;
                this.Y = value.Top;
                this.X1 = value.Right;
                this.Y1 = value.Bottom;
                endMoveRedim();
                if (ConnectionPoint != null)
                    ConnectionPoint.rePosition();
            }
        }

        [CategoryAttribute("Image"), DescriptionAttribute("File image")]
        public Bitmap img
        {
            get
            {
                return _img;
            }
            set
            {
                _img = value;
            }
        }

        [CategoryAttribute("Image"), DescriptionAttribute("Trasparent")]
        public bool Trasparent
        {
            get
            {
                return _trasparent;
            }
            set
            {
                _trasparent = value;
            }
        }

        [DescriptionAttribute("Rotation angle")]
        public int Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                _rotation = value;
            }
        }

        public override void AddGp(GraphicsPath gp, int dx, int dy, float zoom)
        {
            gp.AddRectangle(new RectangleF((this.X + dx) * zoom, (this.Y + dy) * zoom, (this.X1 - this.X) * zoom, (this.Y1 - this.Y) * zoom));
        }


        public override BaseObject Copy()
        {
            ImgBox newE = new ImgBox(drawingPanel, this.X, this.Y, this.X1, this.Y1);
            newE.penColor = this.penColor;
            newE.penWidth = this.penWidth;
            newE.fillColor = this.fillColor;
            newE.filled = this.filled;
            newE.bIsLine = this.bIsLine;
            newE.alpha = this.alpha;
            newE.dashStyle = this.dashStyle;
            newE.showBorder = this.showBorder;
            newE.Trasparent = this.Trasparent;
            newE.Rotation = this.Rotation;

            newE.img = this.img;

            return newE;
        }

        public override void CopyFrom(BaseObject ele)
        {
            this.copyStdProp(ele, this);
            this.img = ((ImgBox)ele).img;
        }

        public override void Select()
        {
            this.undoEle = this.Copy();
        }

        public void Load_IMG()
        {
            string f_name = this.imgLoader();
            if (f_name != null)
            {
                try
                {
                    Bitmap loadTexture = new Bitmap(f_name);
                    this.img = loadTexture;
                }
                catch { }
            }
        }

        private string imgLoader()
        {
            try
            {
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.Title = "Load background image";
                dlg.Filter = "Image Files(*.BMP;*.JPG;*.PNG;*.GIF)|*.BMP;*.JPG;*.PNG;*.GIF|All files (*.*)|*.*";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    return (dlg.FileName);
                }
            }
            catch { }
            return null;
        }



        public override void Draw(Graphics g, int dx, int dy, float zoom)
        {
            RectangleF rect = new RectangleF((this.X + dx) * zoom, (this.Y + dy) * zoom, (X1 - X) * zoom, (Y1 - Y) * zoom);
            //g.FillRectangle(new SolidBrush(Color.FromArgb(100, 255, 0, 0)), rect);

            System.Drawing.Pen myPen = new System.Drawing.Pen(this.penColor, scaledPenWidth(zoom));
            myPen.DashStyle = this.dashStyle;

            if (this.bSelected)
            {
                myPen.Color = Color.Red;
                myPen.Color = this.Trasparency(myPen.Color, 120);
                myPen.Width = myPen.Width + 1;
            }

            if (img != null)
            {
                Color backColor = this.img.GetPixel(0, 0);
                int dim = (int)System.Math.Sqrt(img.Width * img.Width + img.Height * img.Height);
                Bitmap curBitmap = new Bitmap(dim, dim);
                Graphics curG = Graphics.FromImage(curBitmap);

                if (this.Rotation > 0)
                {
                    Matrix mX = new Matrix();
                    mX.RotateAt(this.Rotation, new PointF(curBitmap.Width / 2, curBitmap.Height / 2));
                    curG.Transform = mX;
                    mX.Dispose();
                }
                curG.DrawImage(img, (dim - img.Width) / 2, (dim - img.Height) / 2, img.Width, img.Height);

                if (this.Trasparent)
                    curBitmap.MakeTransparent(backColor);

                curG.Save();
                g.DrawImage(curBitmap, (this.X + dx) * zoom, (this.Y + dy) * zoom, (this.X1 - this.X) * zoom, (this.Y1 - this.Y) * zoom);

                curG.Dispose();
                curBitmap.Dispose();
            }

            if (Name != null)
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Near;

                Font font = new Font("Arial", 8 * zoom);
                SizeF size = g.MeasureString(Name, font);
                if ((X1 - X) * zoom > size.Width)
                    size.Width = (X1 - X) * zoom;
                g.DrawString(Name, font, new SolidBrush(Color.Black), new RectangleF((this.X + dx) * zoom, (this.Y1 + dy) * zoom, size.Width, size.Height), stringFormat);
                font.Dispose();
                stringFormat.Dispose();
            }

            if (this.showBorder || bSelected)
                g.DrawRectangle(myPen, rect.X, rect.Y, rect.Width, rect.Height);

            myPen.Dispose();
            base.Draw(g, dx, dy, zoom);
        }
    }
}