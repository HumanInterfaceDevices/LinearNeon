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
			startRadiusAngle -= Convert.ToInt32(startRadiusAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check
			sweepAngle -= Convert.ToInt32(sweepAngle / (pi * 2)) * (pi * 2); // mathematical overcircle check

			double toCenterAngle = arcClockwise ? startAngle + (pi / 2) : startAngle - (pi / 2); // direction to center
			if (toCenterAngle > (pi * 2)) toCenterAngle -= pi * 2;
			if (toCenterAngle < 0) toCenterAngle += pi * 2;
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
			if (endRadiusAngle > (pi * 2)) endRadiusAngle -= pi * 2;
			if (endRadiusAngle < 0) endRadiusAngle += pi * 2;

			arcArray[0, 3] = Convert.ToSingle(xCenter + (Math.Cos(endRadiusAngle) * radius)) - arcArray[0, 0]; // calculate end point
			arcArray[1, 3] = Convert.ToSingle(yCenter + (Math.Sin(endRadiusAngle) * radius)) - arcArray[1, 0];

			double endAngle = arcClockwise ? endRadiusAngle - (pi / 2) : endRadiusAngle + (pi / 2); // calculate end bezier point
			if (endAngle > (pi * 2d)) endAngle -= pi * 2;
			if (endAngle < 0d) endAngle += pi * 2;
			arcArray[0, 2] = Convert.ToSingle(arcArray[0, 3] + (Math.Cos(endAngle) * bezierLength));
			arcArray[1, 2] = Convert.ToSingle(arcArray[1, 3] + (Math.Sin(endAngle) * bezierLength));

			return arcArray;
		}
		public static float[,] reverseArcArray(float[,] ArcArray)
		{
			float [,] arcArray = ArcArray;
			float [,] swapArray = new float [2,4];
			swapArray[0, 0] = arcArray[0, 3] - arcArray[0, 3];
			swapArray[1, 0] = arcArray[1, 3] - arcArray[1, 3];
			swapArray[0, 1] = arcArray[0, 2] - arcArray[0, 3];
			swapArray[1, 1] = arcArray[1, 2] - arcArray[1, 3];
			swapArray[0, 2] = arcArray[0, 1] - arcArray[0, 3];
			swapArray[1, 2] = arcArray[1, 1] - arcArray[1, 3];
			swapArray[0, 3] = arcArray[0, 0] - arcArray[0, 3];
			swapArray[1, 3] = arcArray[1, 0] - arcArray[1, 3];
			return swapArray;
		}
	}
}
