using Entities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace BizFlow
{
    /// <summary>
    /// Fix-placed dynamic banner for AlephATM application.
    /// </summary>
    internal class AdBanner : PictureBox
    {
        private static readonly NLog.Logger Log = NLog.LogManager.GetLogger("LOG");
        private Bitmap _idleImage { get; set; }
        private List<Bitmap> _images { get; set; } = new List<Bitmap> { };

        /// <summary>
        /// Creates a banner for the AlephATM application.
        /// </summary>
        /// <param name="height">Banner' height</param>
        /// <param name="dock">Dock style from WForms</param>
        public AdBanner(int height, DockStyle dock) : base()
        {
            Height = height;
            Dock = dock;

            // Retrieving idle image.
            try
            {
                Bitmap bitmap = new Bitmap($"{Const.appPath}Screens\\Graphics\\banner\\idle.png");
                _idleImage = bitmap;
            }
            catch (Exception ex)
            {
                Log.Warn($"idle.png for banner couldn't be found: {ex.Message}");
            }

            // Retrieving banner image list.
            string[] pngFiles = Directory.GetFiles($"{Const.appPath}Screens\\Graphics\\banner\\images");

            if (pngFiles != null)
            {
                for (int i = 0; i < pngFiles.Length; i++)
                {
                    try
                    {
                        Bitmap bitmap = new Bitmap(pngFiles[i]);
                        _images.Add(bitmap);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"Error loading PNG file: {pngFiles[i]}. {ex.Message}");
                    }
                }
            }
            else
            {
                Log.Warn("There are no images for banner usage.");
            }

            SetIdleImage();
        }

        /// <summary>
        /// Set the idle image on out of transaction screens.
        /// </summary>
        public void SetIdleImage()
        {
            if (_idleImage != null)
            {
                Image = _idleImage;
            }
        }

        /// <summary>
        /// Set an image when on a transaction.
        /// If there is more than one image, randomly changes on screen navigation.
        /// </summary>
        public void SetTxnImage()
        {
            if (_images.Count > 1)
            {
                Random random = new Random();
                Bitmap chosenImage;

                do
                {
                    chosenImage = _images[random.Next(_images.Count)];
                } while (chosenImage == Image);

                Image = chosenImage;
            }
            else if (_images.Count == 1)
            {
                Image = _images[0];
            }

        }
    }
}
