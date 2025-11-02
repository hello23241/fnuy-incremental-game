using System.Drawing;
using System.Windows.Forms;

namespace WinFormsApp1.UI.Controllers
{
    public sealed class FormDragController
    {
        private readonly Form _form;
        private bool _dragging;
        private Point _cursorStart;
        private Point _formStart;

        public FormDragController(Form form)
        {
            _form = form;
        }

        public void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = true;
            _cursorStart = Cursor.Position;
            _formStart = _form.Location;
        }

        public void OnMouseMove(MouseEventArgs e)
        {
            if (!_dragging) return;
            Point diff = Point.Subtract(Cursor.Position, new Size(_cursorStart));
            _form.Location = Point.Add(_formStart, new Size(diff));
        }

        public void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _dragging = false;
        }
    }
}