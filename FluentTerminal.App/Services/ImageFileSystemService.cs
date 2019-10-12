﻿using FluentTerminal.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Controls;

namespace FluentTerminal.App.Services
{
    public class ImageFileSystemService : IImageFileSystemService
    {
        public async Task RemoveTemporaryBackgroundThemeImage()
        {
            var folder = await ApplicationData.Current
                                     .LocalCacheFolder
                                     .TryGetItemAsync("BackgroundThemeTmp");

            if (folder == null)
            {
                return;
            }

            var backgroundThemeTmpFolder =
                await ApplicationData.Current
                                     .LocalCacheFolder
                                     .GetFolderAsync(
                                            "BackgroundThemeTmp");

            await backgroundThemeTmpFolder.DeleteAsync();
        }

        public async Task<ImageFile> ImportTemporaryImageFile(IEnumerable<string> fileTypes)
        {
            var picker = new FileOpenPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };

            foreach (var fileType in fileTypes)
            {
                picker.FileTypeFilter.Add(fileType);
            }

            var file = await picker.PickSingleFileAsync();

            if (file != null)
            {
                var backgroundThemeTmpFolder =
                    await ApplicationData.Current
                                         .LocalCacheFolder
                                         .CreateFolderAsync(
                                                "BackgroundThemeTmp",
                                                CreationCollisionOption.OpenIfExists);

                var item = await backgroundThemeTmpFolder.TryGetItemAsync(file.Name);

                if (item == null)
                {
                    var storageFile = await file.CopyAsync(backgroundThemeTmpFolder, file.Name);

                    return new ImageFile(
                        storageFile.DisplayName,
                        storageFile.FileType,
                        $@"{backgroundThemeTmpFolder.Path}\{storageFile.Name}");
                }

                return new ImageFile(
                    file.DisplayName,
                    file.FileType,
                    $@"{backgroundThemeTmpFolder.Path}\{item.Name}");
            }

            return null;
        }

        public async Task RemoveImportedImage(string fileName)
        {
            var backgroundThemeFolder = await ApplicationData.Current.RoamingFolder
                .CreateFolderAsync("BackgroundTheme", CreationCollisionOption.OpenIfExists);

            var item = await backgroundThemeFolder.TryGetItemAsync(fileName);

            if (item != null)
            {
                var file = await backgroundThemeFolder.GetFileAsync(fileName);
                await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
            }
        }

        public string EncodeImage(ImageFile imageFile)
        {
            return Convert.ToBase64String(System.IO.File.ReadAllBytes(imageFile.Path));

            //using (Image image = Image.FromFile(imageFile.Path))
            //{
            //    using (MemoryStream m = new MemoryStream())
            //    {
            //        image.Save(m, image.RawFormat);
            //        byte[] imageBytes = m.ToArray();

            //        // Convert byte[] to Base64 String
            //        string base64String = Convert.ToBase64String(imageBytes);
            //        return base64String;
            //    }
            //}
        }
    }
}
