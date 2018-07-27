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
		private static int MaxFilePages = 8;

		/// <summary>
		/// DEPRECTATED.
		/// Convierte todos los archivos TIFF encontrados en la estructura de carpetas indicada y
		/// los convierte a archivos PNG
		/// </summary>
		/// <param name="sourceFolderName">La carpeta raíz a buscar</param>
		/// <param name="targetFolderName">La carpeta destino donde se guardarán los PNG</param>
		public void Convert(string sourceFolderName, string targetFolderName)
		{
			List<String> files = GetTifFiles(sourceFolderName);

			for (int f = 0; f < files.Count; f++)
			{
				Console.WriteLine(files[f]);
				ConvertTifToPng(files[f], targetFolderName);
			}
		}

        /// <summary>
		/// NO SE USA.
		/// Convierte el archivo TIFF que se encuentra en el pathFile a un archivo PNG
		/// </summary>
		/// <param name="sourceFolderName">La carpeta raíz a buscar</param>
		public List<string> Convert(string filePath)
        {
            string dirName = new DirectoryInfo(filePath).Name;
            return ConvertTifToPng(filePath, dirName);
        }

        /// <summary>
        /// Consigue todos los archivos TIFF en un a carpeta y sus subcarpetas
        /// </summary>
        /// <param name="sourceFolderName">La carpeta raíz a buscar</param>
        /// <returns>Una lista de archivos y su full path</returns>
        private static List<string> GetTifFiles(string sourceFolderName)
		{
			List<String> files = new List<String>();
			try
			{
				foreach (string f in Directory.GetFiles(sourceFolderName, "*.tif"))
				{
					files.Add(f);
				}
				foreach (string d in Directory.GetDirectories(sourceFolderName))
				{
					files.AddRange(GetTifFiles(d)); 
				}
			}
			catch (System.Exception e) { }

			return files;
		}

		/// <summary>
		/// Convierte un archivo TIFF en uno o varios PNG. Cada PNG tiene un máximo de MaxFilePages folios.
		/// </summary>
		/// <param name="tifFile">El archivo a convertir</param>
		/// <param name="targetFolder">La carpeta de destino</param>
		/// <returns>La lista de los archivos png generados</returns>
		private static List<string> ConvertTifToPng(string tifFile, string targetFolder)
		{
			System.IO.Directory.CreateDirectory(targetFolder);

			Image png = Image.FromFile(tifFile);

			List<string> pngFiles = new List<string>();
			string pngFileName;
			Image tif = Image.FromFile(tifFile);
			int pages = tif.GetFrameCount(FrameDimension.Page);

			for (int fileNumber = 0; fileNumber <= Math.Ceiling(new Decimal(pages / MaxFilePages)); fileNumber++)
			{
				Console.WriteLine("File: " + (fileNumber + 1).ToString() + " - Page: " + (fileNumber * MaxFilePages + 1).ToString());
				tif.SelectActiveFrame(FrameDimension.Page, fileNumber * MaxFilePages);
				png = (Image) tif.Clone();

				for (int p = 1; p < Math.Min(pages - (fileNumber * MaxFilePages), MaxFilePages); p++)
				{
					int CurrentPage = MaxFilePages * fileNumber + p;
					Console.WriteLine("File: " + (fileNumber + 1).ToString() + " - Page: " + (CurrentPage + 1).ToString());
					tif.SelectActiveFrame(FrameDimension.Page, CurrentPage);
					png = MergeTwoImages(png, tif);
				}
				pngFileName = tifFile + "_file" + (fileNumber + 1).ToString() + ".png";
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
