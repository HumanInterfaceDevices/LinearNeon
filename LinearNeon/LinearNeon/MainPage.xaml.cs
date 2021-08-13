using System;
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
		bool createBox = false;

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
			float[] slider = new float[2], swapPoint = new float[2];
			float xCenter, yCenter, lineWidth, lineLength;
			double startAngle, sweepAngle, lastAngle, toCenterAngle, radius;
			string boundingSvgPath, svgPath;
			int circleFraction;

			// bound and formula variables
			slider[0] = Convert.ToSingle(xSlider.Value * 200) - 100;
			slider[1] = Convert.ToSingle(ySlider.Value * 200) - 100;
			startAngle = startSlider.Value * (pi * 2);
			sweepAngle = sweepSlider.Value * (pi / 2);
			lineWidth = Convert.ToSingle(100 * thicknessSlider.Value);
			lineLength = Convert.ToSingle(100 * thicknessSlider.Value);
			radius = radiusSlider.Value * 200;

			// inner thickness of shape can't be larger than arc radius
			if (lineWidth / 2 / radius >= 1)
			{
				lineWidth = Convert.ToSingle(radius * 2);
			}
			

			// create primary arc
			float[,] pathPoint = ArcPlot.arcPointsArray(startAngle, sweepAngle, radius, 0f, clockwise);
			pathPoint[0, 0] += slider[0];
			pathPoint[1, 0] += slider[1];
			svgPath = "m " + pathPoint[0, 0] + " " + pathPoint[1, 0] + " c " + pathPoint[0, 1] + " " + pathPoint[1, 1] + " " + pathPoint[0, 2] + " " + pathPoint[1, 2] + " " + pathPoint[0, 3] + " " + pathPoint[1, 3];
			SKPath bezierPath = SKPath.ParseSvgPathData(svgPath);

			// create outline shape
			boundingSvgPath = ArcOutlineSvg(startAngle, sweepAngle, radius, lineWidth / 2, clockwise, 0, 0, slider[0], slider[1]);
			SKPath boundingBezierPath = SKPath.ParseSvgPathData(boundingSvgPath);

			// visual switches
			if (createBox)
			{
				svgPath = "";
				circleFraction = (sweepAngle != 0) ? Convert.ToInt32(((pi * 2) / sweepAngle)) : 0;
				float currentAngle = Convert.ToSingle(2 * pi / circleFraction);
				for (int i = 0; i < circleFraction; i++)
				{
					lastAngle = -currentAngle * i;
					pathPoint = ArcPlot.arcPointsArray(lastAngle + startAngle, currentAngle, radius, 0f, clockwise);
					pathPoint[0, 0] = Convert.ToSingle(Math.Cos(lastAngle + startAngle) * lineLength);
					pathPoint[1, 0] = Convert.ToSingle(Math.Sin(lastAngle + startAngle) * lineLength);
					svgPath += (i==0) ? " m " + slider[0] + " " + slider[1]: " l " + pathPoint[0, 0] + " " + pathPoint[1, 0];
					svgPath += " c " + pathPoint[0, 1] + " " + pathPoint[1, 1] + " " + pathPoint[0, 2] + " " + pathPoint[1, 2] + " " + pathPoint[0, 3] + " " + pathPoint[1, 3];
				}
				svgPath += " z";
				bezierPath = SKPath.ParseSvgPathData(svgPath);
			}
			if (outline)
			{
				canvas.DrawPath(boundingBezierPath, redStroke);
				canvas.DrawPath(boundingBezierPath, redBlurStroke);
			}
			if (fill)
			{
				// calculate center of circle
				toCenterAngle = clockwise ? startAngle + (pi / 2) : startAngle - (pi / 2);
				if (toCenterAngle > (pi * 2)) toCenterAngle -= pi * 2;
				if (toCenterAngle < 0) toCenterAngle += pi * 2;
				xCenter = Convert.ToSingle(slider[0] + (Math.Cos(toCenterAngle) * radius));
				yCenter = Convert.ToSingle(slider[1] + (Math.Sin(toCenterAngle) * radius));
				SKPoint center = new SKPoint(xCenter, yCenter);
				var laserFill = new SKPaint { Shader = twoPointsConicalShader(center, Convert.ToSingle(radius), lineWidth) };
				canvas.DrawPath(boundingBezierPath, laserFill);
			}
			if (path)
			{
				canvas.DrawPath(bezierPath, greenStroke);
			}
			if (control)
			{
				SKPoint a = new SKPoint(pathPoint[0, 0], pathPoint[1, 0]);
				SKPoint b = new SKPoint(pathPoint[0, 1] + slider[0], pathPoint[1, 1] + slider[1]);
				canvas.DrawLine(a, b, yellowStroke);
				canvas.DrawCircle(a, 1, yellowStroke);
				canvas.DrawCircle(b, 1, yellowStroke);
				a = new SKPoint(pathPoint[0, 2] + slider[0], pathPoint[1, 2] + slider[1]);
				b = new SKPoint(pathPoint[0, 3] + slider[0], pathPoint[1, 3] + slider[1]);
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
									 bool Clockwise = false,
									 float XCenter = 0f, float YCenter = 0f,
									 float ReposX = 0f, float ReposY = 0f)
		{
			string svgOut = "";

			// calculate inner arc start
			double endRadiusAngle = clockwise ? StartAngle + SweepAngle + (pi / 2) : StartAngle - SweepAngle - (pi / 2);
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0d) endRadiusAngle += pi * 2d;

			float[,] arcPointOuter = ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, RadiusOffset, Clockwise, XCenter, YCenter);

			arcPointOuter[0, 0] += ReposX;
			arcPointOuter[1, 0] += ReposY;

			svgOut += " m " + arcPointOuter[0, 0] + " " + arcPointOuter[1, 0] + " c " + arcPointOuter[0, 1] + " " + arcPointOuter[1, 1] + " " + arcPointOuter[0, 2] + " " + arcPointOuter[1, 2] + " " + arcPointOuter[0, 3] + " " + arcPointOuter[1, 3];

			float[,] arcPointInner = ArcPlot.reverseArcArray(ArcPlot.arcPointsArray(StartAngle, SweepAngle, Radius, -RadiusOffset, Clockwise, XCenter, YCenter));

			arcPointInner[0, 0] = Convert.ToSingle(Math.Cos(endRadiusAngle) * 2 * RadiusOffset);
			arcPointInner[1, 0] = Convert.ToSingle(Math.Sin(endRadiusAngle) * 2 * RadiusOffset);

			svgOut += " l " + arcPointInner[0, 0] + " " + arcPointInner[1, 0] + " c " + arcPointInner[0, 1] + " " + arcPointInner[1, 1] + " " + arcPointInner[0, 2] + " " + arcPointInner[1, 2] + " " + arcPointInner[0, 3] + " " + arcPointInner[1, 3] + " z";
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
		private void boxButton_Clicked(object sender, EventArgs e)
		{
			createBox = !createBox;
		}
	}
}