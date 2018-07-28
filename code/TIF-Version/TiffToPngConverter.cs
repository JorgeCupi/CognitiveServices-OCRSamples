using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace WPF
{
	/// <summary>
	/// Convierte un archivo TIFF en un conjunto de PNG.
	/// Genera un archivo por cada MaxFilePages páginas
	/// Reduce la resolución del archivo.
	/// Puede quitar píxeles superiores e inferiores de cada página (desactivado)
	/// Puede ignorar las páginas en blanco (desactivado por ser muy lento)
	/// </summary>
	public class TiffToPngConverter
	{
		private static int MaxFilePages = 16;

        /// <summary>
		/// Convierte el archivo TIFF que se encuentra en el pathFile a uno o mas archivos PNG dependiendo a su tamaño.
		/// </summary>
		/// <param name="sourceFolderName">La carpeta raíz a buscar</param>
		public List<string> ConvertTiffToPngFiles(string filePath)
        {
            Image png = Image.FromFile(filePath);

            List<string> pngFiles = new List<string>();
            string pngFileName;
            Image tif = Image.FromFile(filePath);
            int pages = tif.GetFrameCount(FrameDimension.Page);
            int maxPages = int.Parse(Math.Ceiling(new Decimal(pages / MaxFilePages)).ToString());
            for (int fileNumber = 0; fileNumber <= maxPages; fileNumber++)
            {
                Console.WriteLine("File: " + (fileNumber + 1).ToString() + " - Page: " + (fileNumber * MaxFilePages + 1).ToString());
                tif.SelectActiveFrame(FrameDimension.Page, fileNumber * MaxFilePages);
                png = (Image)tif.Clone();

                for (int p = 1; p < Math.Min(pages - (fileNumber * MaxFilePages), MaxFilePages); p++)
                {
                    int CurrentPage = MaxFilePages * fileNumber + p;
                    Console.WriteLine("File: " + (fileNumber + 1).ToString() + " - Page: " + (CurrentPage + 1).ToString());
                    tif.SelectActiveFrame(FrameDimension.Page, CurrentPage);
                    png = MergeTwoImages(png, tif);
                }
                pngFileName = filePath + "_file" + (fileNumber + 1).ToString() + ".png";
                png.Save(pngFileName, ImageFormat.Png);
                pngFiles.Add(pngFileName);
                Console.WriteLine("Saving file: " + pngFileName);
            }
            return pngFiles;
        }

		/// <summary>
		/// Añade una imagen debajo de otra.
		/// </summary>
		/// <param name="firstImage">La imagen superior.</param>
		/// <param name="secondImage">La imagen inferior.</param>
		/// <returns>La imagen mergeada</returns>
		private static Bitmap MergeTwoImages(Image firstImage, Image secondImage)
		{
			int outputImageWidth = firstImage.Width > secondImage.Width ? firstImage.Width : secondImage.Width;
			int outputImageHeight = firstImage.Height + secondImage.Height + 1;

			Bitmap outputImage = new Bitmap(outputImageWidth, outputImageHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

			using (Graphics graphics = Graphics.FromImage(outputImage))
			{
				graphics.DrawImage(firstImage, new Rectangle(new Point(), firstImage.Size),
					new Rectangle(new Point(), firstImage.Size), GraphicsUnit.Pixel);
				graphics.DrawImage(secondImage, new Rectangle(new Point(0, firstImage.Height + 1), secondImage.Size),
					new Rectangle(new Point(), secondImage.Size), GraphicsUnit.Pixel);
			}

			return outputImage;
		}

		/// <summary>
		/// NO SE ESTÁ USANDO. Ver si aplica para todos los tif o sólo para algunos templates.
		/// Quita 150 píxeles de arriba y de abajo de la página..
		/// </summary>
		/// <param name="srcImage">La imagen a recortar.</param>
		/// <returns>La imagen recortada</returns>
		private static Bitmap CropImage(Image srcImage)
		{
			Rectangle cropArea = new Rectangle(0, 150, srcImage.Width, srcImage.Height - 300);
			Bitmap bmpImage = new Bitmap(srcImage);
			return ShrinkImage(bmpImage.Clone(cropArea, bmpImage.PixelFormat));
		}

		/// <summary>
		/// DEPRECTATED. El método es efectivo pero tarda mucho. No se está usando.
		/// Evalúa si la página está en blanco
		/// </summary>
		/// <param name="srcImage">La imagen a evaluar.</param>
		/// <returns>True si la página es mayormente blanca</returns>
		private static bool IsBlank(Image srcImage)
		{
			double stdDev = GetStdDev(new Bitmap(srcImage));
			return stdDev < 1300000;
		}

		/// <summary>
		/// Devuelve la desviación estandar de los valores de los píxeles.
		/// </summary>
		/// <param name="srcImage">La imagen a evaluar.</param>
		/// <returns>Desviación estandar</returns>
		private static double GetStdDev(Bitmap srcImage)
		{
			double total = 0, totalVariance = 0;
			int count = 0;
			double stdDev = 0;

			BitmapData bmData = srcImage.LockBits(new Rectangle(0, 0, srcImage.Width, srcImage.Height), ImageLockMode.ReadOnly, srcImage.PixelFormat);
			int stride = bmData.Stride;
			IntPtr Scan0 = bmData.Scan0;
			unsafe
			{
				byte* p = (byte*)(void*)Scan0;
				int nOffset = stride - srcImage.Width * 3;
				for (int y = 0; y < srcImage.Height; ++y)
				{
					for (int x = 0; x < srcImage.Width; ++x)
					{
						count++;

						byte blue = p[0];
						byte green = p[1];
						byte red = p[2];

						int pixelValue = Color.FromArgb(0, red, green, blue).ToArgb();
						total += pixelValue;
						double avg = total / count;
						totalVariance += Math.Pow(pixelValue - avg, 2);
						stdDev = Math.Sqrt(totalVariance / count);

						p += 3;
					}
					p += nOffset;
				}
			}

			srcImage.UnlockBits(bmData);

			return stdDev;
		}

		/// <summary>
		/// Reduce el tamaño de las imagenes al 50%
		/// </summary>
		/// <param name="imgToResize">La imagen a reducir</param>
		/// <returns>Imagen reducida.</returns>
		private static Bitmap ShrinkImage(Bitmap imgToResize)
		{
			return ResizeImage(imgToResize, new Size(imgToResize.Width / 2, imgToResize.Height / 2));
		}

		/// <summary>
		/// Ajusta el tamaño de la imagen al Size proporcionado
		/// </summary>
		/// <param name="imgToResize">La imagen a ajustar</param>
		/// <param name="size">El nuevo tamaño</param>
		/// <returns>Imagen adaptada.</returns>
		private static Bitmap ResizeImage(Bitmap imgToResize, Size size)
		{
			return new Bitmap(imgToResize, size);
		}
	}
}
