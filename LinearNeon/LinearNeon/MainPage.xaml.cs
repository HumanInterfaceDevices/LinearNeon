using System;
//using System.IO;
//using System.Reflection;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using Xamarin.Forms;
using SkiaSharp;
using SkiaSharp.Views.Forms;

namespace LinearNeon
{
	public partial class MainPage : ContentPage
	{
		// Constants
		private const double pi = Math.PI;

		// Paints
		SKPaint blackFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Black,
			TextSize = 8,
			IsAntialias = true
		};

		SKPaint slateFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.DarkSlateGray,
			TextSize = 8,
			IsAntialias = true
		};

		SKPaint whiteFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.White,
			IsAntialias = true
		};

		SKPaint limeFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.LimeGreen,
			IsAntialias = true
		};

		SKPaint redFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Red,
			TextSize = 8,
			IsAntialias = true
		};

		SKPaint blackStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Black,
			StrokeWidth = 10,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};

		SKPaint slateStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.DarkSlateGray,
			StrokeWidth = 10,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};

		SKPaint blueStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Blue,
			StrokeWidth = 10,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};

		SKPaint redStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Red,
			StrokeWidth = 1,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};

		SKPaint yellowStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Yellow,
			StrokeWidth = 1,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true
		};
		public MainPage()
		{
			InitializeComponent();

			Device.StartTimer(TimeSpan.FromSeconds(1f / 60), () =>
			{
				canvasView.InvalidateSurface();
				return true;
			});
		}
		private void canvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			//Prepare canvas
			SKSurface surface = e.Surface;
			SKCanvas canvas = surface.Canvas;
			canvas.DrawPaint(blackFill);

			//Set Transforms
			int width = e.Info.Width;
			int height = e.Info.Height;
			canvas.Translate(width / 2, height / 2);
			canvas.Scale(Math.Min(width / 210f, height / 520f));

			//draw grid
			for (int i = -200; i <= 200; i = i + 10)
			{
				for (int j = -200; j <= 200; j = j + 10)
				{
					int r = ((i % 100 == 0) & (j % 100 == 0)) ? 2 : 1;
					SKPoint dot = new SKPoint(i, j);
					canvas.DrawCircle(dot, r, limeFill);
				}
			}

			//Declare variables
			float[,] conPts = new float[2, 4];
			float xCenter, yCenter;
			double startAngle, sweepAngle, endAngle, toRadiusAngle, startRadiusAngle, endRadiusAngle;
			double radius, circleFraction, bezierLength;
			bool clockwise; // direction
			string svgAbsoluteBezierPath, svgRelativeBezierPath;

			// Bound variables
			conPts[0, 0] = Convert.ToSingle(xSlider.Value * 200f) - 100;
			conPts[1, 0] = Convert.ToSingle(ySlider.Value * 200f) - 100;
			startAngle = startSlider.Value * (pi * 2);
			sweepAngle = sweepSlider.Value * (pi / 2);
			radius = radiusSlider.Value * 200;
			clockwise = Convert.ToBoolean(Convert.ToInt32(Math.Round(clockwiseSlider.Value)));

			// Calculate bezier length
			circleFraction = pi * 2d / sweepAngle;
			bezierLength = radius * 4d / 3d * Math.Tan(pi / (2d * circleFraction));

			// calculate start bezier point
			conPts[0, 1] = Convert.ToSingle(conPts[0, 0] + (Math.Cos(startAngle) * bezierLength));
			conPts[1, 1] = Convert.ToSingle(conPts[1, 0] + (Math.Sin(startAngle) * bezierLength));

			// calculate arc center
			toRadiusAngle = clockwise ? startAngle + (pi / 2d) : startAngle - (pi / 2d);
			if (toRadiusAngle > (2d * pi)) toRadiusAngle -= 2d * pi;
			if (toRadiusAngle < 0d) toRadiusAngle += 2d * pi;
			xCenter = Convert.ToSingle(conPts[0, 0] + (Math.Cos(toRadiusAngle) * radius));
			yCenter = Convert.ToSingle(conPts[1, 0] + (Math.Sin(toRadiusAngle) * radius));

			// calculate arc end
			startRadiusAngle = toRadiusAngle + pi;
			//if (startRadiusAngle > (2d * Math.PI)) startRadiusAngle -= 2d * Math.PI;
			startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2);
			endRadiusAngle = clockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle;
			if (endRadiusAngle > (2d * pi)) endRadiusAngle -= 2d * pi;
			if (endRadiusAngle < 0d) endRadiusAngle += 2d * pi;
			conPts[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius));
			conPts[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius));

			// calculate end bezier point
			endAngle = clockwise ? endRadiusAngle - (pi / 2d) : endRadiusAngle + (pi / 2d);
			if (endAngle > (2d * pi)) endAngle -= 2d * pi;
			if (endAngle < 0d) endAngle += 2d * pi;
			conPts[0, 2] = Convert.ToSingle(conPts[0, 3] + (Math.Cos(endAngle) * bezierLength));
			conPts[1, 2] = Convert.ToSingle(conPts[1, 3] + (Math.Sin(endAngle) * bezierLength));

			// assemble bezier string for arc
			svgAbsoluteBezierPath = " C " + conPts[0, 1] + " " + conPts[1, 1] +
				" " + conPts[0, 2] + " " + conPts[1, 2] + " " + conPts[0, 3] + " " + conPts[1, 3];
			SKPath bezierPath = SKPath.ParseSvgPathData("M " + conPts[0, 0] + " " + conPts[1, 0] + svgAbsoluteBezierPath);

			// calculate and draw tangents
			SKPoint point1 = new SKPoint(conPts[0, 0], conPts[1, 0]);
			SKPoint point2 = new SKPoint(Convert.ToSingle(conPts[0, 0] - (Math.Cos(startAngle) * 50)), Convert.ToSingle(conPts[1, 0] - (Math.Sin(startAngle) * 50)));
			canvas.DrawLine(point1, point2, blueStroke);
			point1 = new SKPoint(conPts[0, 3], conPts[1, 3]);
			point2 = new SKPoint(Convert.ToSingle(conPts[0, 3] - (Math.Cos(endAngle) * 50)), Convert.ToSingle(conPts[1, 3] - (Math.Sin(endAngle) * 50)));
			canvas.DrawLine(point1, point2, blueStroke);

			//Draw absolute arc and control points
			canvas.DrawPath(bezierPath, slateStroke);

			point1 = new SKPoint(conPts[0, 0], conPts[1, 0]);
			canvas.DrawCircle(point1, 3, redFill);
			point2 = new SKPoint(conPts[0, 1], conPts[1, 1]);
			canvas.DrawCircle(point2, 3, redFill);
			canvas.DrawLine(point1, point2, yellowStroke);

			point1 = new SKPoint(conPts[0, 2], conPts[1, 2]);
			canvas.DrawCircle(point1, 3, redFill);
			point2 = new SKPoint(conPts[0, 3], conPts[1, 3]);
			canvas.DrawCircle(point2, 3, redFill);
			canvas.DrawLine(point1, point2, yellowStroke);

			SKPoint arcCenter = new SKPoint(xCenter, yCenter);
			canvas.DrawCircle(arcCenter, 2, redFill);

			canvas.DrawText("M " + conPts[0, 0] + " " + conPts[1, 0] + svgAbsoluteBezierPath, -170, 220, slateFill);

			// convert literal to relative
			for (int i = 3; i > 0; --i)
			{
				conPts[0, i] -= conPts[0, 0];
				conPts[1, i] -= conPts[1, 0];
			}
			svgRelativeBezierPath = " c " + conPts[0, 1] + " " + conPts[1, 1] +
				" " + conPts[0, 2] + " " + conPts[1, 2] + " " + conPts[0, 3] + " " + conPts[1, 3];
			bezierPath = SKPath.ParseSvgPathData("m " + conPts[0, 0] + " " + conPts[1, 0] + svgRelativeBezierPath);

			// draw relative arc
			canvas.DrawPath(bezierPath, redStroke);
			canvas.DrawText("m " + conPts[0, 0] + " " + conPts[1, 0] + svgRelativeBezierPath, -170, 240, redFill);
			// return svgRelativeBezierPath;
		}
	}
}