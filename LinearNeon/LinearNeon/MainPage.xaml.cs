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
		bool clockwise;

		// Paints
		SKPaint blackFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Black,
			TextSize = 8,
			IsAntialias = true
		};

		SKPaint redFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Red,
			TextSize = 8,
			IsAntialias = true
		};

		SKPaint blueFill = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.LightBlue,
			TextSize = 8,
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

			// canvas transforms
			int width = e.Info.Width;
			int height = e.Info.Height;
			canvas.Translate(width / 2, height / 2);
			canvas.Scale(Math.Min(width / 210f, height / 520f));

			// variables
			float[,] outlinePoint = new float[2, 8], pointsTotal = new float[2, 1];
			float[] slider = new float[2];
			float xCenter, yCenter, lineWidth;
			double startAngle, sweepAngle, endAngle, toCenterAngle, startRadiusAngle, endRadiusAngle;
			double radius, outerRadius, innerRadius, circleFraction, bezierLength;
			string svgPath;

			// bound and formula variables
			slider[0] = Convert.ToSingle(xSlider.Value * 200) - 100;
			slider[1] = Convert.ToSingle(ySlider.Value * 200) - 100;
			startAngle = startSlider.Value * (pi * 2);
			sweepAngle = sweepSlider.Value * (pi / 2);
			lineWidth = Convert.ToSingle(0.1 +(99.9 * thicknessSlider.Value));
			radius = (radiusSlider.Value * 200)+(lineWidth/2);

			// calculate center of circle
			toCenterAngle = clockwise ? startAngle + (pi / 2) : startAngle - (pi / 2);
			if (toCenterAngle > (pi * 2)) toCenterAngle -= pi * 2;
			if (toCenterAngle < 0) toCenterAngle += pi * 2;
			xCenter = Convert.ToSingle(slider[0] + (Math.Cos(toCenterAngle) * radius));
			yCenter = Convert.ToSingle(slider[1] + (Math.Sin(toCenterAngle) * radius));
			// radiuses, radii?
			outerRadius = radius + (lineWidth / 2);
			innerRadius = radius - (lineWidth / 2);

			// calculate outer start
			startRadiusAngle = clockwise ? startAngle - (pi / 2) : startAngle + (pi / 2);
			startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2); 
			
			// mathematical overcircle check - replaces: if (startRadiusAngle > (pi * 2d)) startRadiusAngle -= pi * 2d
			outlinePoint[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * outerRadius));
			outlinePoint[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * outerRadius));
			// calculate outer bezier length                                 
			circleFraction = pi * 2 / sweepAngle;
			bezierLength = outerRadius * 4 / 3 * Math.Tan(pi / (2 * circleFraction));
			// calculate outer start bezier point
			outlinePoint[0, 1] = Convert.ToSingle(outlinePoint[0, 0] + (Math.Cos(startAngle) * bezierLength));
			outlinePoint[1, 1] = Convert.ToSingle(outlinePoint[1, 0] + (Math.Sin(startAngle) * bezierLength));
			// calculate outer arc end
			endRadiusAngle = clockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle;
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;
			outlinePoint[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * outerRadius));
			outlinePoint[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * outerRadius));
			// calculate outer end bezier point
			endAngle = clockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2);
			if (endAngle > (pi * 2d)) endAngle -= pi * 2;
			if (endAngle < 0d) endAngle += pi * 2;
			outlinePoint[0, 2] = Convert.ToSingle(outlinePoint[0, 3] + (Math.Cos(endAngle) * bezierLength));
			outlinePoint[1, 2] = Convert.ToSingle(outlinePoint[1, 3] + (Math.Sin(endAngle) * bezierLength));

			// calculate inner start
			startRadiusAngle = clockwise ? startAngle - (pi / 2) : startAngle + (pi / 2);
			startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check - replaces: if (startRadiusAngle > (pi * 2d)) startRadiusAngle -= pi * 2d
			outlinePoint[0, 7] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * innerRadius));
			outlinePoint[1, 7] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * innerRadius));
			// calculate inner bezier length                                 
			circleFraction = pi * 2 / sweepAngle;
			bezierLength = innerRadius * 4 / 3 * Math.Tan(pi / (2 * circleFraction));
			// calculate inner start bezier point
			outlinePoint[0, 6] = Convert.ToSingle(outlinePoint[0, 7] + (Math.Cos(startAngle) * bezierLength));
			outlinePoint[1, 6] = Convert.ToSingle(outlinePoint[1, 7] + (Math.Sin(startAngle) * bezierLength));
			// calculate inner arc end
			endRadiusAngle = clockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle;
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;
			outlinePoint[0, 4] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * innerRadius));
			outlinePoint[1, 4] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * innerRadius));
			// calculate inner end bezier point
			endAngle = clockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2);
			if (endAngle > (pi * 2d)) endAngle -= pi * 2;
			if (endAngle < 0d) endAngle += pi * 2;
			outlinePoint[0, 5] = Convert.ToSingle(outlinePoint[0, 4] + (Math.Cos(endAngle) * bezierLength));
			outlinePoint[1, 5] = Convert.ToSingle(outlinePoint[1, 4] + (Math.Sin(endAngle) * bezierLength));

			// Convert absolute to relative
			pointsTotal[0, 0] = outlinePoint[0, 0];
			pointsTotal[1, 0] = outlinePoint[1, 0];
			for (int i = 3; i > 0; --i)
			{
				outlinePoint[0, i] -= outlinePoint[0, 0];
				outlinePoint[1, i] -= outlinePoint[1, 0];
			}
			pointsTotal[0, 0] += outlinePoint[0, 3];
			pointsTotal[1, 0] += outlinePoint[1, 3];
			outlinePoint[0, 4] -= pointsTotal[0, 0];
			outlinePoint[1, 4] -= pointsTotal[1, 0];
			pointsTotal[0, 0] += outlinePoint[0, 4];
			pointsTotal[1, 0] += outlinePoint[1, 4];
			for (int i = 7; i > 4; --i)
			{
				outlinePoint[0, i] -= pointsTotal[0, 0];
				outlinePoint[1, i] -= pointsTotal[1, 0];
			}

			// create conical paint
			SKPoint center = new SKPoint(xCenter, yCenter);
			var conicalPaint = new SKPaint { Shader = twoPointsConicalShader(center, Convert.ToSingle(radius), lineWidth) };


			//draw grid
			for (int i = -200; i <= 200; i = i + 10)
			{
				for (int j = -200; j <= 200; j = j + 10)
				{
					int r = ((i % 100 == 0) & (j % 100 == 0)) ? 5 : 2;
					SKPoint dot = new SKPoint(i, j);
					canvas.DrawCircle(dot, r, blueFill);
				}
			}



			// concatenate, parse and draw relative svg
			svgPath = "m " + outlinePoint[0, 0] + " " + outlinePoint[1, 0] + " c " + outlinePoint[0, 1] + " " + outlinePoint[1, 1] + " " + outlinePoint[0, 2] + " " + outlinePoint[1, 2] + " " + outlinePoint[0, 3] + " " + outlinePoint[1, 3] +
					 " l " + outlinePoint[0, 4] + " " + outlinePoint[1, 4] + " c " + outlinePoint[0, 5] + " " + outlinePoint[1, 5] + " " + outlinePoint[0, 6] + " " + outlinePoint[1, 6] + " " + outlinePoint[0, 7] + " " + outlinePoint[1, 7] + " z";
			SKPath bezierPath = SKPath.ParseSvgPathData(svgPath);
			canvas.DrawPath(bezierPath, conicalPaint);
			canvas.DrawText(svgPath, -240, 240, redFill);

			// return svgRelativeBezierPath;
		}

		private SKShader twoPointsConicalShader(SKPoint center, float radius, float width)
		{
			var colors = new SKColor[]
			{
				new SKColor(0, 0, 0, 0),
				new SKColor(160, 0, 0, 200),
				new SKColor(255, 0, 0, 240),
				new SKColor(255, 128, 128, 128),
				new SKColor(255, 255, 255, 255),
				new SKColor(255, 128, 128, 128),
				new SKColor(255, 0, 0, 240),
				new SKColor(160, 0, 0, 200),
				new SKColor(0, 0, 0, 0)
			};
			var shader = SKShader.CreateTwoPointConicalGradient(
			center, radius + (width/2f), center, radius - (width/2f), colors, null,
		SKShaderTileMode.Decal);
			return shader;
		}

		private void clockwiseButton_Clicked(object sender, EventArgs e)
		{
			clockwise = !clockwise;
		}
	}
}