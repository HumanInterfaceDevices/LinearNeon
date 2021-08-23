using System;
using System.Collections.Generic;
using System.Text;
using SkiaSharp;

namespace LinearNeon
{
	class ArcPlot
	{
		private const double pi = Math.PI;

		public static float[,] arcPointsArray(double StartAngle, double SweepAngle, double Radius, double RadiusOffset = 0d,
											  bool Clockwise = false, float XCenter = 0f, float YCenter = 0f)
		{
			double radius = Radius, startAngle = StartAngle, sweepAngle = SweepAngle, radiusOffset = RadiusOffset;
			bool arcClockwise = Clockwise;
			float xCenter = XCenter, yCenter = YCenter;

			double startRadiusAngle = arcClockwise ? startAngle - (pi / 2) : startAngle + (pi / 2); // angle from radius to start point
			startRadiusAngle = circleCheck(startRadiusAngle);
			sweepAngle = circleCheck(sweepAngle);

			double toCenterAngle = arcClockwise ? startAngle + (pi / 2) : startAngle - (pi / 2); // direction to center
			toCenterAngle = circleCheck(toCenterAngle);
			if (XCenter == 0f) xCenter = Convert.ToSingle(Math.Cos(toCenterAngle) * radius); // center coordinates
			if (YCenter == 0f) yCenter = Convert.ToSingle(Math.Sin(toCenterAngle) * radius);

			radius += radiusOffset;

			float[,] arcArray = new float[2, 4];
			arcArray[0, 0] = Convert.ToSingle(xCenter + (Math.Cos(startRadiusAngle) * radius)); // relocate start point after offset added
			arcArray[1, 0] = Convert.ToSingle(yCenter + (Math.Sin(startRadiusAngle) * radius));

			double circleFraction = pi * 2 / sweepAngle; // portion of a complete circle in sweep
			double bezierLength = radius * 4 / 3 * Math.Tan(pi / (2 * circleFraction)); // calculate bezier length

			arcArray[0, 1] = Convert.ToSingle(arcArray[0, 0] + (Math.Cos(startAngle) * bezierLength)) - arcArray[0, 0]; // calculate start bezier point
			arcArray[1, 1] = Convert.ToSingle(arcArray[1, 0] + (Math.Sin(startAngle) * bezierLength)) - arcArray[1, 0];

			double endRadiusAngle = arcClockwise ? startRadiusAngle + sweepAngle : startRadiusAngle - sweepAngle; // Which way and how far
			endRadiusAngle = circleCheck(endRadiusAngle);

			arcArray[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius)) - arcArray[0, 0]; // calculate end point
			arcArray[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius)) - arcArray[1, 0];

			double endAngle = arcClockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2); // calculate end bezier point
			endAngle = circleCheck(endAngle);
			arcArray[0, 2] = Convert.ToSingle(arcArray[0, 3] + (Math.Cos(endAngle) * bezierLength));
			arcArray[1, 2] = Convert.ToSingle(arcArray[1, 3] + (Math.Sin(endAngle) * bezierLength));


			return arcArray;
		}
		public static float[,] reverseArcArray(float[,] ArcArray)
		{
			float[,] arcArray = ArcArray;
			float[,] swapArray = new float[2, 4];
			swapArray[0, 0] = 0f;
			swapArray[1, 0] = 0f;
			swapArray[0, 1] = arcArray[0, 2] - arcArray[0, 3];
			swapArray[1, 1] = arcArray[1, 2] - arcArray[1, 3];
			swapArray[0, 2] = arcArray[0, 1] - arcArray[0, 3];
			swapArray[1, 2] = arcArray[1, 1] - arcArray[1, 3];
			swapArray[0, 3] = -arcArray[0, 3];
			swapArray[1, 3] = -arcArray[1, 3];
			return swapArray;
		}

		public static float circleCheck(float AngleIn)
		{
			float angleOut = AngleIn;
			if (angleOut > (pi * 2)) angleOut -= Convert.ToSingle(pi * 2);
			if (angleOut < 0) angleOut += Convert.ToSingle(pi * 2);
			return angleOut;
		}
		public static double circleCheck(double AngleIn)
		{
			double angleOut = AngleIn;
			if (angleOut > (pi * 2)) angleOut -= pi * 2;
			if (angleOut < 0) angleOut += pi * 2;
			return angleOut;
		}
		public static FullArc arcDivider(double SweepAngle)
		{
			FullArc output = new FullArc();
			output.count = Convert.ToInt32(Math.Truncate(SweepAngle / (pi / 2))) + 1;
			output.angle = SweepAngle / output.count;
			return output;
		}
	}
	class FullArc
	{
		public double angle;
		public int count;
	}

}
