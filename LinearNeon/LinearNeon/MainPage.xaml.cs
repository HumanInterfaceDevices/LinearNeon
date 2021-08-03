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

		// buttons
		bool clockwise = false;
		bool fill = false;
		bool outline = true;
		bool path = true;
		bool control = true;
		bool isGrid = false;

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
			Color = SKColors.PaleGreen,
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

		SKPaint redBlurStroke = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = SKColors.Red,
			StrokeWidth = 1,
			StrokeCap = SKStrokeCap.Round,
			IsAntialias = true,
			MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Outer, 5)
		};

		SKPaint greenStroke = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Green,
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
		private void CanvasView_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
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
			float[,] outlinePoint = new float[2, 8], controlPoint = new float[2, 8], pathPoint = new float[2, 4], pointsTotal = new float[2, 1];
			float[] slider = new float[2];
			float xCenter, yCenter, lineWidth;
			double startAngle, sweepAngle, endAngle, toCenterAngleAngle, startRadiusAngle, endRadiusAngle;
			double radius, circleFraction, bezierLength;
			string boundingSvgPath, svgPath;

			// bound and formula variables
			slider[0] = Convert.ToSingle(xSlider.Value * 200) - 100;
			slider[1] = Convert.ToSingle(ySlider.Value * 200) - 100;
			startAngle = startSlider.Value * (pi * 2);
			sweepAngle = sweepSlider.Value * (pi / 2);
			lineWidth = Convert.ToSingle(100 * thicknessSlider.Value);
			radius = (radiusSlider.Value * 200) + (lineWidth / 2);

			// calculate center of circle
			toCenterAngleAngle = clockwise ? startAngle + (pi / 2) : startAngle - (pi / 2);
			if (toCenterAngleAngle > (pi * 2)) toCenterAngleAngle -= pi * 2;
			if (toCenterAngleAngle < 0) toCenterAngleAngle += pi * 2;
			xCenter = Convert.ToSingle(slider[0] + (Math.Cos(toCenterAngleAngle) * radius));
			yCenter = Convert.ToSingle(slider[1] + (Math.Sin(toCenterAngleAngle) * radius));

			//// calculate path start
			//startRadiusAngle = clockwise ? startAngle - (pi / 2) : startAngle + (pi / 2);
			//startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check - replaces: if (startRadiusAngle > (pi * 2d)) startRadiusAngle -= pi * 2d
			//pathPoint[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * radius));
			//pathPoint[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * radius));

			//// calculate path bezier length                                 
			//circleFraction = pi * 2 / sweepAngle;
			//bezierLength = radius * 4 / 3 * Math.Tan(pi / (2 * circleFraction));

			//// calculate path start bezier point
			//pathPoint[0, 1] = Convert.ToSingle(pathPoint[0, 0] + (Math.Cos(startAngle) * bezierLength));
			//pathPoint[1, 1] = Convert.ToSingle(pathPoint[1, 0] + (Math.Sin(startAngle) * bezierLength));

			//// calculate path arc end
			//endRadiusAngle = clockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle;
			//if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			//if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;
			//pathPoint[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius));
			//pathPoint[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius));

			//// calculate path end bezier point
			//endAngle = clockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2);
			//if (endAngle > (pi * 2d)) endAngle -= pi * 2;
			//if (endAngle < 0d) endAngle += pi * 2;
			//pathPoint[0, 2] = Convert.ToSingle(pathPoint[0, 3] + (Math.Cos(endAngle) * bezierLength));
			//pathPoint[1, 2] = Convert.ToSingle(pathPoint[1, 3] + (Math.Sin(endAngle) * bezierLength));

			//draw grid
			if (isGrid)
			{
				for (int i = -400; i <= 400; i = i + 10)
				{
					for (int j = -400; j <= 400; j = j + 10)
					{
						int r = ((i % 100 == 0) & (j % 100 == 0)) ? 3 : 1;
						SKPoint dot = new SKPoint(i, j);
						canvas.DrawCircle(dot, r, blueFill);
					}
				}
			}

			// concatenate, parse and draw svgs
			pathPoint = ArcPlot.arcPointsArray(startAngle, sweepAngle, radius, 0f, clockwise);

			pathPoint[0, 0] += slider[0];
			pathPoint[1, 0] += slider[1];


			boundingSvgPath = ArcOutlineSvg(startAngle, sweepAngle, radius, lineWidth / 2, clockwise, false, 0, 0, slider[0], slider[1]);

			svgPath = "m " + pathPoint[0, 0] + " " + pathPoint[1, 0] + " c " + pathPoint[0, 1] + " " + pathPoint[1, 1] + " " + pathPoint[0, 2] + " " + pathPoint[1, 2] + " " + pathPoint[0, 3] + " " + pathPoint[1, 3];
			SKPath boundingBezierPath = SKPath.ParseSvgPathData(boundingSvgPath);

			if (outline)
			{
				canvas.DrawPath(boundingBezierPath, redStroke);
				canvas.DrawPath(boundingBezierPath, redBlurStroke);
			}

			if (fill)
			{
				SKPoint center = new SKPoint(xCenter, yCenter);
				var laserFill = new SKPaint { Shader = twoPointsConicalShader(center, Convert.ToSingle(radius), lineWidth) };
				canvas.DrawPath(boundingBezierPath, laserFill);
			}

			if (path)
			{
				SKPath bezierPath = SKPath.ParseSvgPathData(svgPath);
				canvas.DrawPath(bezierPath, greenStroke);
			}
			if (control)
			{
				SKPoint a = new SKPoint(controlPoint[0, 0] + slider[0], controlPoint[1, 0] + slider[1]);
				SKPoint b = new SKPoint(controlPoint[0, 1] + slider[0], controlPoint[1, 1] + slider[1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 2] + slider[0], controlPoint[1, 2] + slider[1]);
				b = new SKPoint(controlPoint[0, 3] + slider[0], controlPoint[1, 3] + slider[1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 4] + slider[0], controlPoint[1, 4] + slider[1]);
				b = new SKPoint(controlPoint[0, 5] + slider[0], controlPoint[1, 5] + slider[1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 6] + slider[0], controlPoint[1, 6] + slider[1]);
				b = new SKPoint(controlPoint[0, 7] + slider[0], controlPoint[1, 7] + slider[1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
			}
			canvas.DrawText(boundingSvgPath, -240, 240, redFill);
			canvas.DrawText(svgPath, -240, 255, redFill);
		}
		private SKShader twoPointsConicalShader(SKPoint center, float radius, float width)
		{
			var colors = new SKColor[]
			{
				new SKColor(0, 0, 0, 0),
				new SKColor(160, 0, 0, 128),
				new SKColor(255, 0, 0, 255),
				new SKColor(255, 0, 0, 255),
				new SKColor(255, 128, 128, 228),
				new SKColor(255, 255, 255, 255),
				new SKColor(255, 128, 128, 228),
				new SKColor(255, 0, 0, 255),
				new SKColor(255, 0, 0, 255),
				new SKColor(160, 0, 0, 128),
				new SKColor(0, 0, 0, 0)
			};
			var shader = SKShader.CreateTwoPointConicalGradient(
			center, radius + (width / 2f), center, radius - (width / 2f), colors, null,
		SKShaderTileMode.Decal);
			return shader;
		}
		private string ArcOutlineSvg(double StartAngle, double SweepAngle,
									 double Radius, double RadiusOffset = 0d,
									 bool Clockwise = false, bool Inner = false,
									 float XCenter = 0f, float YCenter = 0f,
									 float ReposX = 0f, float ReposY = 0f)
		{
			string svgOut = "";
			float reposX = ReposX;
			float reposY = ReposY;

			// calculate inner arc start
			double endRadiusAngle = clockwise ? StartAngle + SweepAngle + (pi / 2) : StartAngle - SweepAngle - (pi / 2);
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;

			float[,] arcPoint = ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, RadiusOffset, Clockwise, XCenter, YCenter);

			arcPoint[0, 0] += reposX;
			arcPoint[1, 0] += reposY;

			svgOut += " m " + arcPoint[0, 0] + " " + arcPoint[1, 0] + " c " + arcPoint[0, 1] + " " + arcPoint[1, 1] + " " + arcPoint[0, 2] + " " + arcPoint[1, 2] + " " + arcPoint[0, 3] + " " + arcPoint[1, 3];

			arcPoint = ArcPlot.reverseArcArray(ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, -RadiusOffset, Clockwise, XCenter, YCenter));

			arcPoint[0, 0] = Convert.ToSingle(Math.Cos(endRadiusAngle) * 2 * RadiusOffset);
			arcPoint[1, 0] = Convert.ToSingle(Math.Sin(endRadiusAngle) * 2 * RadiusOffset);

			svgOut += " l " + arcPoint[0, 0] + " " + arcPoint[1, 0] + " c " + arcPoint[0, 1] + " " + arcPoint[1, 1] + " " + arcPoint[0, 2] + " " + arcPoint[1, 2] + " " + arcPoint[0, 3] + " " + arcPoint[1, 3] + " z";
			return svgOut;
		}

		private void clockwiseButton_Clicked(object sender, EventArgs e)
		{
			clockwise = !clockwise;
		}
		private void outlineButton_Clicked(object sender, EventArgs e)
		{
			outline = !outline;
		}
		private void fillButton_Clicked(object sender, EventArgs e)
		{
			fill = !fill;
		}
		private void pathButton_Clicked(object sender, EventArgs e)
		{
			path = !path;
		}
		private void controlButton_Clicked(object sender, EventArgs e)
		{
			control = !control;
		}
		private void gridButton_Clicked(object sender, EventArgs e)
		{
			isGrid = !isGrid;
		}
	}
}