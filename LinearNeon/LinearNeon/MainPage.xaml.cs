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
			double radius, outerRadius, innerRadius, circleFraction, bezierLength;
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

			// calculate path start
			startRadiusAngle = clockwise ? startAngle - (pi / 2) : startAngle + (pi / 2);
			startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check - replaces: if (startRadiusAngle > (pi * 2d)) startRadiusAngle -= pi * 2d
			pathPoint[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * radius));
			pathPoint[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * radius));

			// calculate path bezier length                                 
			circleFraction = pi * 2 / sweepAngle;
			bezierLength = radius * 4 / 3 * Math.Tan(pi / (2 * circleFraction));

			// calculate path start bezier point
			pathPoint[0, 1] = Convert.ToSingle(pathPoint[0, 0] + (Math.Cos(startAngle) * bezierLength));
			pathPoint[1, 1] = Convert.ToSingle(pathPoint[1, 0] + (Math.Sin(startAngle) * bezierLength));

			// calculate path arc end
			endRadiusAngle = clockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle;
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;
			pathPoint[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius));
			pathPoint[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius));

			// calculate path end bezier point
			endAngle = clockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2);
			if (endAngle > (pi * 2d)) endAngle -= pi * 2;
			if (endAngle < 0d) endAngle += pi * 2;
			pathPoint[0, 2] = Convert.ToSingle(pathPoint[0, 3] + (Math.Cos(endAngle) * bezierLength));
			pathPoint[1, 2] = Convert.ToSingle(pathPoint[1, 3] + (Math.Sin(endAngle) * bezierLength));


			// create conical paint
			SKPoint center = new SKPoint(xCenter, yCenter);
			var conicalPaint = new SKPaint { Shader = twoPointsConicalShader(center, Convert.ToSingle(radius), lineWidth) };

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
			boundingSvgPath = ArcOutlineSvg(startAngle, sweepAngle, radius, lineWidth / 2, clockwise);
			svgPath = "M " + pathPoint[0, 0] + " " + pathPoint[1, 0] + " C " + pathPoint[0, 1] + " " + pathPoint[1, 1] + " " + pathPoint[0, 2] + " " + pathPoint[1, 2] + " " + pathPoint[0, 3] + " " + pathPoint[1, 3];
			SKPath boundingBezierPath = SKPath.ParseSvgPathData(boundingSvgPath);

			if (fill) canvas.DrawPath(boundingBezierPath, conicalPaint);
			if (outline)
			{
				canvas.DrawPath(boundingBezierPath, redStroke);
				//canvas.DrawPath(boundingBezierPath, redBlurStroke);
			}
			if (path)
			{
				SKPath bezierPath = SKPath.ParseSvgPathData(svgPath);
				canvas.DrawPath(bezierPath, greenStroke);
			}
			if (control)
			{
				SKPoint a = new SKPoint(controlPoint[0, 0], controlPoint[1, 0]);
				SKPoint b = new SKPoint(controlPoint[0, 1], controlPoint[1, 1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 2], controlPoint[1, 2]);
				b = new SKPoint(controlPoint[0, 3], controlPoint[1, 3]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 4], controlPoint[1, 4]);
				b = new SKPoint(controlPoint[0, 5], controlPoint[1, 5]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(controlPoint[0, 6], controlPoint[1, 6]);
				b = new SKPoint(controlPoint[0, 7], controlPoint[1, 7]);
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
		private string ArcOutlineSvg(double StartAngle, double SweepAngle, double Radius, double RadiusOffset = 0d,
									 bool Clockwise = false, float XCenter = 0f, float YCenter = 0f, bool Inner = false)
		{
			string svgOut = "";
			float[,] arcPoint = ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, RadiusOffset, Clockwise);
			svgOut += " m " + arcPoint[0, 0] + " " + arcPoint[1, 0] + " c " + arcPoint[0, 1] + " " + arcPoint[1, 1] + " " + arcPoint[0, 2] + " " + arcPoint[1, 2] + " " + arcPoint[0, 3] + " " + arcPoint[1, 3];
			arcPoint = ArcPlot.arcPointsArray(StartAngle + SweepAngle, -SweepAngle, Radius, -RadiusOffset, !Clockwise);
			//arcPoint = ArcPlot.reverseArcArray(ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, -RadiusOffset, Clockwise));
			svgOut += " m " + arcPoint[0, 0] + " " + arcPoint[1, 0] + " c " + arcPoint[0, 1] + " " + -arcPoint[1, 1] + " " + arcPoint[0, 2] + " " + -arcPoint[1, 2] + " " + arcPoint[0, 3] + " " + -arcPoint[1, 3];
			return svgOut;
		}
		//double radius = Radius, startAngle = StartAngle, sweepAngle = SweepAngle, radiusOffset = RadiusOffset;
		//bool arcClockwise = Clockwise;
		//float xCenter = XCenter, yCenter = YCenter;
		//double startRadiusAngle = arcClockwise ? startAngle - (pi / 2) : startAngle + (pi / 2); // angle from radius to start point
		//startRadiusAngle -= Math.Truncate(startRadiusAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check
		//sweepAngle -= Math.Truncate(sweepAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check
		//double toCenterAngle = arcClockwise ? startAngle + (pi / 2) : startAngle - (pi / 2); // direction to center
		//if (toCenterAngle > (pi * 2)) toCenterAngle -= pi * 2;
		//if (toCenterAngle < 0) toCenterAngle += pi * 2;
		//if (xCenter == 0f) xCenter = Convert.ToSingle(Math.Cos(toCenterAngle) * radius); // center coordinates
		//if (yCenter == 0f) yCenter = Convert.ToSingle(Math.Sin(toCenterAngle) * radius);
		//int arcSections = Convert.ToInt32(Math.Truncate(sweepAngle / ((pi / 2) + 0.00000001)) + 1); // calculate number of sections not greater than 1/4 circle
		//double sectionAngle = sweepAngle / arcSections; // actual section angle
		//double circleFraction = pi * 2 / sectionAngle; // portion of a complete circle in section
		//radius += radiusOffset;

		//float[,] arcPoint = new float[2, 4];
		//arcPoint[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * radius));
		//arcPoint[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * radius));

		//svgOut += "l " + arcPoint[0, 0] + " " + arcPoint[1, 0];

		//// calculate bezier length
		//double bezierLength = radius * 4 / 3 * Math.Tan(pi / (2 * circleFraction));
		//for (int i = 0; i < arcSections; ++i) // repeat for each section to complete arc
		//{
		//	// calculate start bezier point
		//	arcPoint[0, 1] = Convert.ToSingle(arcPoint[0, 0] + (Math.Cos(startAngle) * bezierLength));
		//	arcPoint[1, 1] = Convert.ToSingle(arcPoint[1, 0] + (Math.Sin(startAngle) * bezierLength));
		//	double endRadiusAngle = arcClockwise ? startRadiusAngle + sectionAngle : startRadiusAngle - sectionAngle;
		//	if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
		//	if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;
		//	arcPoint[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius));
		//	arcPoint[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius));
		//	// calculate end bezier point
		//	double endAngle = arcClockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2);
		//	if (endAngle > (pi * 2d)) endAngle -= pi * 2;
		//	if (endAngle < 0d) endAngle += pi * 2;
		//	arcPoint[0, 2] = Convert.ToSingle(arcPoint[0, 3] + (Math.Cos(endAngle) * bezierLength));
		//	arcPoint[1, 2] = Convert.ToSingle(arcPoint[1, 3] + (Math.Sin(endAngle) * bezierLength));
		//	if (Inner)
		//	{
		//		float swapFloat = arcPoint[0, 0]; arcPoint[0, 0] = arcPoint[0, 3]; arcPoint[0, 3] = swapFloat;
		//		swapFloat = arcPoint[0, 1]; arcPoint[0, 1] = arcPoint[0, 2]; arcPoint[0, 2] = swapFloat;
		//		swapFloat = arcPoint[1, 0]; arcPoint[1, 0] = arcPoint[1, 3]; arcPoint[1, 3] = swapFloat;
		//		swapFloat = arcPoint[1, 1]; arcPoint[1, 1] = arcPoint[1, 2]; arcPoint[1, 2] = swapFloat;
		//		for (int j = 3; j >= 0; --j)
		//		{
		//			arcPoint[0, j] -= arcPoint[0, 0];
		//			arcPoint[1, j] -= arcPoint[1, 0];
		//		}
		//	}

		//	//prepare for output and next loop
		//	startRadiusAngle = endRadiusAngle;
		//	startAngle = arcClockwise ? startAngle + sectionAngle : startAngle - sectionAngle;
		//	svgOut += " c " + arcPoint[0, 1] + " " + arcPoint[1, 1] + " " + arcPoint[0, 2] + " " + arcPoint[1, 2] + " " + arcPoint[0, 3] + " " + arcPoint[1, 3];
		//	arcPoint[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * radius));
		//	arcPoint[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * radius));
		//}
		//arcPointsArray(double StartAngle, double SweepAngle, double Radius, double RadiusOffset = 0d, bool Clockwise = false, float XCenter = 0f, float YCenter = 0f)


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