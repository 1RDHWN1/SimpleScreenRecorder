using System;
using System.Drawing;
using System.Windows.Forms;

namespace SimpleScreenRecorder;

public class RegionSelector : Form
{
    private Rectangle selectedRegion = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1920, 1080);
    private Point startPoint = Point.Empty;
    private bool isSelecting = false;

    public RegionSelector()
    {
        this.Text = "Select Recording Area";
        this.FormBorderStyle = FormBorderStyle.None;
        this.BackColor = Color.Black;
        this.Opacity = 0.3;
        this.TopMost = true;
        this.ShowInTaskbar = false;
        this.WindowState = FormWindowState.Maximized;

        this.MouseDown += RegionSelector_MouseDown;
        this.MouseMove += RegionSelector_MouseMove;
        this.MouseUp += RegionSelector_MouseUp;
        this.Paint += RegionSelector_Paint;
        this.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) this.Close();
        };
    }

    private void RegionSelector_MouseDown(object? sender, MouseEventArgs e)
    {
        isSelecting = true;
        startPoint = e.Location;
    }

    private void RegionSelector_MouseMove(object? sender, MouseEventArgs e)
    {
        if (isSelecting)
        {
            selectedRegion = new Rectangle(
                Math.Min(startPoint.X, e.X),
                Math.Min(startPoint.Y, e.Y),
                Math.Abs(e.X - startPoint.X),
                Math.Abs(e.Y - startPoint.Y)
            );
            this.Invalidate();
        }
    }

    private void RegionSelector_MouseUp(object? sender, MouseEventArgs e)
    {
        isSelecting = false;
        if (selectedRegion.Width > 50 && selectedRegion.Height > 50)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }

    private void RegionSelector_Paint(object? sender, PaintEventArgs e)
    {
        using (var pen = new Pen(Color.Red, 2))
        {
            e.Graphics.DrawRectangle(pen, selectedRegion);
        }
        string info = $"{selectedRegion.Width} x {selectedRegion.Height}";
        e.Graphics.DrawString(info, this.Font, Brushes.White, 
            selectedRegion.X + 5, selectedRegion.Y + 5);
    }

    public Rectangle GetSelectedRegion() => selectedRegion;
}
