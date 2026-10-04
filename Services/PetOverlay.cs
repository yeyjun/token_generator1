using Microsoft.Maui.Graphics;

namespace TokenGenerator.Services;

public class PetOverlay : WindowOverlay
{
    readonly PetElement _pet = new();
    readonly IDispatcherTimer _timer;
    TaskCompletionSource? _runCompletion;

    public PetOverlay(IWindow window) : base(window)
    {
        AddWindowElement(_pet);

        _timer = Application.Current!.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromMilliseconds(80);
        _timer.Tick += OnTick;
    }

    public Task RunAcrossScreenAsync()
    {
        _runCompletion = new TaskCompletionSource();
        _pet.X = -PetElement.Width;
        _pet.Y = (float)Window.Height - PetElement.Height - 32;
        _timer.Start();
        return _runCompletion.Task;
    }

    void OnTick(object? sender, EventArgs e)
    {
        _pet.X += 9f;
        _pet.LegFrameA = !_pet.LegFrameA;
        Invalidate();

        if (_pet.X > (float)Window.Width)
        {
            _timer.Stop();
            _runCompletion?.TrySetResult();
        }
    }

    class PetElement : IWindowOverlayElement
    {
        public const float Width = 46f;
        public const float Height = 40f;

        static readonly Color BodyColor = Color.FromArgb("#FFA94D");
        static readonly Color ShadeColor = Color.FromArgb("#E8890C");

        public float X;
        public float Y;
        public bool LegFrameA = true;

        public bool Contains(Point point) => false;

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var x = X;
            var y = Y;

            canvas.StrokeColor = ShadeColor;
            canvas.StrokeSize = 4;
            if (LegFrameA)
            {
                canvas.DrawLine(x + 8, y + 30, x + 5, y + 40);
                canvas.DrawLine(x + 30, y + 30, x + 33, y + 40);
            }
            else
            {
                canvas.DrawLine(x + 8, y + 30, x + 11, y + 40);
                canvas.DrawLine(x + 30, y + 30, x + 27, y + 40);
            }

            var tail = new PathF();
            tail.MoveTo(x, y + 20);
            tail.CurveTo(x - 14, y + 16, x - 14, y, x - 2, y - 2);
            canvas.DrawPath(tail);

            canvas.FillColor = BodyColor;
            canvas.FillRoundedRectangle(x + 2, y + 12, 36, 20, 9);

            canvas.FillEllipse(x + 26, y - 2, 20, 18);

            var leftEar = new PathF();
            leftEar.MoveTo(x + 26, y + 2);
            leftEar.LineTo(x + 29, y - 8);
            leftEar.LineTo(x + 33, y + 2);
            leftEar.Close();
            canvas.FillColor = BodyColor;
            canvas.FillPath(leftEar);

            var rightEar = new PathF();
            rightEar.MoveTo(x + 38, y + 2);
            rightEar.LineTo(x + 42, y - 8);
            rightEar.LineTo(x + 45, y + 2);
            rightEar.Close();
            canvas.FillPath(rightEar);

            canvas.FillColor = Colors.Black;
            canvas.FillEllipse(x + 39, y + 5, 3, 3);
        }
    }
}
