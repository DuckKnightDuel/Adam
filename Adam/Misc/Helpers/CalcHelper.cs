﻿using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Adam
{
    public static class CalcHelper
    {
        /// <summary>
        /// Returns the hypotenuse of two sides or "pythagorates" a vector.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float GetPythagoras(float a, float b)
        {
            double x = (double)a;
            double y = (double)b;

            double x2 = Math.Pow(x, 2);
            double y2 = Math.Pow(y, 2);

            double sqroot = Math.Sqrt(x2 + y2);
            return (float)sqroot;
        }

        /// <summary>
        /// Returns the unit vector as a Vector2 of another Vector2
        /// </summary>
        /// <param name="vector2">The Vector2 that the unit vector will be extraced from</param>
        /// <returns></returns>
        public static Vector2 GetUnitVector(Vector2 vector2)
        {
            float magnitude = GetPythagoras(vector2.X, vector2.Y);
            Vector2 unitVector = new Vector2(vector2.X / magnitude, vector2.Y / magnitude);

            return unitVector;
        }

        /// <summary>
        /// Returns an instance of a public class as a byte array.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static byte[] ToByteArray(object source)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, source);
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Converts a byte array into an object.
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public static object ConvertToObject(byte[] array)
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream(array))
            {
                var obj = formatter.Deserialize(stream);
                return obj;
            }

        }

        /// <summary>
        /// Takes a number and applies the ratio of screen expansion to it. This is the same as GetScreenScale, except it multiplies the ratio by two, which is useful for UI elements that were made at half size.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int ApplyUiRatio(int number)
        {
            return (int)(ApplyScreenScale(number) * 2);
        }

        /// <summary>
        /// DEPRECATED DO NOT USE
        /// Takes a number and applies the ratio of the screen expansion to it.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int ApplyHeightRatio(int number)
        {
            return ApplyScreenScale(number);
        }

        /// <summary>
        /// Returns a random x value from this rectangle's x-coord.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static int GetRandomX(Rectangle rect)
        {
            return Main.Random.Next(rect.X, rect.X + rect.Width);
        }

        /// <summary>
        /// Returns a random y value from this rectangle's y-coord.
        /// </summary>
        /// <param name="rect"></param>
        /// <returns></returns>
        public static int GetRandomY(Rectangle rect)
        {
            return Main.Random.Next(rect.Y, rect.Y + rect.Height);
        }

        /// <summary>
        /// Returns the ratio of the screen to the game depending on what the resolution is.
        ///  </summary>
        /// <returns></returns>
        public static float GetScreenScale()
        {
            return ((float)Main.UserResWidth / Main.DefaultResWidth);
        }


        /// <summary>
        /// Applies the ratio of the screen to the game to the number.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static int ApplyScreenScale(float number)
        {
            return (int)(GetScreenScale() * number);
        }

        /// <summary>
        /// Returns an integer for position animation.
        /// </summary>
        /// <param name="time">Time since start of animation.</param>
        /// <param name="startValue">Starting position.</param>
        /// <param name="delta">Distance to be travelled.</param>
        /// <param name="duration">Target duration of transition.</param>
        /// <returns></returns>
        public static float EaseInAndOut(float time, float startValue, float delta, float duration)
        {
            if (time > duration)
            {
                return startValue + delta;
            }
            time /= duration / 2;
            if (time < 1) return (delta / 2) * (time * time) + startValue;
            time--;
            return -delta / 2 * (time * (time - 2) - 1) + startValue;
        }

        public static void SharpAnimationY(int start, int finish, ref Rectangle rectangle, ref float velocityY)
        {
            int direction = 1;
            int middle = 0;
            if (finish > start)
            {
                middle = start + (finish - start) / 2;
            }
            else
            {
                middle = finish + (finish - start) / 2;
                direction = -1;
            }


            if (rectangle.Y < finish)
            {
                if (rectangle.Y < middle)
                {
                    velocityY -= 13f * direction;
                }
                else velocityY += 13f * direction;
            }

            else
            {
                velocityY = 0;
                rectangle.Y = finish;
            }
        }
    }
}