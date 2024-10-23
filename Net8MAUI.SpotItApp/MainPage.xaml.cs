using CommunityToolkit.Maui.Storage;
using Net8MAUI.SpotItApp.Data;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using Image = Microsoft.Maui.Controls.Image;
using Point = System.Drawing.Point;


namespace Net8MAUI.SpotItApp
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        public int Count { get; set; }

        private string selectedSaveLocation { get; set; }
        public string SelectedSaveLocation
        {
            get { return selectedSaveLocation; }
            set
            {
                if (selectedSaveLocation != value)
                {
                    selectedSaveLocation = value;
                    OnPropertyChanged(nameof(SelectedSaveLocation));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public bool RotationEnabled { get; set; }
        private string imageNumber { get; set; }
        public string ImageNumber
        {
            get { return imageNumber; }
            set
            {
                if (imageNumber != value)
                {
                    imageNumber = value;
                    OnPropertyChanged(nameof(ImageNumber));
                }
            }
        }

        private ObservableCollection<ImageSource> _imageCollection = [];

        public ObservableCollection<ImageSource> ImageCollection
        {
            get { return _imageCollection; }
            set
            {
                if (_imageCollection != value)
                {
                    _imageCollection = value;
                    OnPropertyChanged(nameof(ImageCollection));
                }
            }
        }
        public ObservableCollection<ImageSource> MergeCollection = new();


        readonly Patterns patterns = new();
        int[][] MergingPattern { get; set; }


        public MainPage()
        {
            InitializeComponent();
            BindingContext = this;
            ImageNumber = "0";
            RotationEnabled = false;
            SelectedSaveLocation = string.Empty;
        }

        async void OnImportImagesClicked(object sender, EventArgs e)
        {
            Count = 0;
            ImageNumber = Count.ToString();
            ImageCollection.Clear();
            MergeCollection.Clear();
            Thumbnails.Clear();

            var result = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Select images",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
            {
                return;
            }

            if (result != null)
            {
                foreach (var file in result)
                {
                    DisplayThumbnail(file);
                    ImageCollection.Add(file.FullPath);
                    MergeCollection.Add(file.FullPath);
                    Count++;
                }
                ImageNumber = Count.ToString();
            }
        }

        void OpenOutputFolder()
        {
            System.Diagnostics.Process.Start("explorer.exe", SelectedSaveLocation);

        }

        void DisplayThumbnail(FileResult file)
        {
            using var stream = File.OpenRead(file.FullPath);
            using var originalBitmap = new Bitmap(stream);
            var resizedBitmap = ResizeImage(originalBitmap, 100, 100);

            var image = new Image
            {
                Source = ImageSource.FromStream(() => GetStreamFromBitmap(resizedBitmap)),
                WidthRequest = 100,
                HeightRequest = 100,
                Aspect = Aspect.AspectFit,
            };
            Thumbnails.Children.Add(image);

            Debug.WriteLine(file.FullPath.ToString());

            ImageCollection.Add(file.FullPath);
        }

        Stream GetStreamFromBitmap(Bitmap bitmap)
        {
            var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        async void OnCreateCards13Clicked(object sender, EventArgs e)
        {
            if (Count == 13)
            {
                MergeImagesAsync();
            }
            else
            {
                await DisplayAlert("Alert", "13 images needed to generate cards, but you added " + Count, "OK");
            }
        }
        async void OnCreateCards31Clicked(object sender, EventArgs e)
        {
            if (Count == 31)
            {
                MergeImagesAsync();
            }
            else
            {
                await DisplayAlert("Alert", "31 images needed to generate cards, but you added " + Count, "OK");
            }
        }
        async void OnCreateCards57Clicked(object sender, EventArgs e)
        {
            if (Count == 57)
            {
                MergeImagesAsync();
            }
            else
            {
                await DisplayAlert("Alert", "57 images needed to generate cards, but you added " + Count, "OK");
            }
        }
        async void OnSelectFolderClicked(object sender, EventArgs e)
        {
            CancellationTokenSource source = new();
            CancellationToken token = source.Token;
            var result = await FolderPicker.Default.PickAsync(token);

            if (result.IsSuccessful)
            {
                SelectedSaveLocation = result.Folder.Path;
            }
        }
        Bitmap ResizeImage(Bitmap originalImage, int maxWidth, int maxHeight)
        {
            //float aspectRatio = (float)originalImage.Width / originalImage.Height;

            int newHeight = 250;
            int newWidth = 250;

            Bitmap resizedImage = new(newWidth, newHeight);

            using (Graphics g = Graphics.FromImage(resizedImage))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
            }

            return resizedImage;
        }

        Bitmap RotateImage(Bitmap originalImage, float rotationAngle)
        {
            Bitmap rotatedImage = new(originalImage.Width, originalImage.Height);

            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(rotatedImage.Width / 2, rotatedImage.Height / 2);
                g.RotateTransform(rotationAngle);
                g.TranslateTransform(-rotatedImage.Width / 2, -rotatedImage.Height / 2);
                g.DrawImage(originalImage, new Point(0, 0));
            }

            return rotatedImage;
        }

        async Task MergeImagesAsync()
        {
            Debug.WriteLine("MERGE STARTED");

            if (string.IsNullOrEmpty(SelectedSaveLocation))
            {
                DisplayAlert("Error", "Please select save location first", "OK");

                return;
            }

            var images = new Bitmap[MergeCollection.Count];

            int index = 0;
            foreach (var imageSource in MergeCollection)
            {
                string filePath = (imageSource as FileImageSource)?.File;
                Debug.WriteLine("File path " + filePath);

                if (!string.IsNullOrEmpty(filePath))
                {
                    using (var stream = File.OpenRead(filePath))
                    using (var originalBitmap = new Bitmap(stream))
                    {
                        images[index] = ResizeImage(originalBitmap, 250, 250);
                    }
                    index++;
                }
                else
                {
                    await DisplayAlert("Error", "File path not available for one or more images.", "OK");
                    return;
                }
            }
            switch (Count)
            {
                case 13:
                    MergingPattern = patterns.mergingPattern13;
                    Debug.WriteLine("MERGE 13");
                    CreateMergedImage(images, 2, 2, 13, 4);
                    break;
                case 31:
                    MergingPattern = patterns.mergingPattern31;
                    Debug.WriteLine("MERGE 31");
                    CreateMergedImage(images, 3, 2, 31, 6);
                    break;
                case 57:
                    MergingPattern = patterns.mergingPattern57;
                    Debug.WriteLine("MERGE 57");
                    CreateMergedImage(images, 4, 2, 57, 8);
                    break;
                default:
                    break;
            }


            async void CreateMergedImage(Bitmap[] images, int gridColumns, int gridRows, int cardIndexLimit, int imagesPerCard)
            {
                // Calculate the size of the merged image
                int squareSize = 360;
                int imageSquareSize = 250;
                int mergedWidth = squareSize * gridColumns;
                int mergedHeight = squareSize * gridRows;

                Random random = new();

                for (int cardIndex = 0; cardIndex < cardIndexLimit; cardIndex++)
                {
                    using var mergedBitmap = new Bitmap(mergedWidth, mergedHeight);
                    using var g = Graphics.FromImage(mergedBitmap);
                    for (int i = 0; i < imagesPerCard; i++)
                    {
                        int imageIndex = MergingPattern[cardIndex][i];

                        int squareX = (i % gridColumns) * squareSize;
                        int squareY = (i / gridColumns) * squareSize;

                        int centerX = squareX + squareSize / 2;
                        int centerY = squareY + squareSize / 2;

                        int rotationAngle = 0;

                        if (RotationEnabled == true)
                        {
                            rotationAngle = random.Next(0, 360);
                        }

                        RectangleF destinationRect = new(centerX - imageSquareSize / 2, centerY - imageSquareSize / 2, imageSquareSize, imageSquareSize);

                        using var matrix = new System.Drawing.Drawing2D.Matrix();
                        matrix.RotateAt(rotationAngle, new System.Drawing.PointF(centerX, centerY));

                        g.Transform = matrix;
                        g.DrawImage(images[imageIndex], destinationRect);
                        g.ResetTransform();
                    }
                    try
                    {
                        using MemoryStream stream = new();
                        mergedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

                        stream.Seek(0, SeekOrigin.Begin);

                        var outputPath = Path.Combine(SelectedSaveLocation, $"MergedImage_{cardIndex + 1}.png");

                        using (var fileStream = File.Create(outputPath))
                        {
                            await stream.CopyToAsync(fileStream);
                        }

                        Debug.WriteLine($"Merged image {cardIndex + 1} saved to {outputPath}");

                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", $"Failed to save merged image {cardIndex + 1}: {ex.Message}", "OK");
                    }
                }
                await DisplayAlert("Success", $"All merged images saved to selected folder", "OK");
                OpenOutputFolder();
            }
        }


    }
}

   
