using ChartSightAI.MVVM.Models;
using ChartSightAI.Popups;
using ChartSightAI.TestData;
using ChartSightAI.Utility;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MT = ChartSightAI.MVVM.Models.Enums.MarketType;
using TD = ChartSightAI.MVVM.Models.Enums.TradeDirection;
using TF = ChartSightAI.MVVM.Models.Enums.TimeFrame;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPredictionVM : ObservableObject
    {
        #region Properties
        [ObservableProperty] private AnalysisSession analysisSession = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsForexSelected))]
        [NotifyPropertyChangedFor(nameof(IsStocksSelected))]
        [NotifyPropertyChangedFor(nameof(IsCryptoSelected))]
        private MT selectedMarketType;

        public bool IsForexSelected => SelectedMarketType == MT.Forex;
        public bool IsStocksSelected => SelectedMarketType == MT.Stocks;
        public bool IsCryptoSelected => SelectedMarketType == MT.Crypto;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsTFH1))]
        [NotifyPropertyChangedFor(nameof(IsTFH4))]
        [NotifyPropertyChangedFor(nameof(IsTFD1))]
        private TF selectedTimeFrame;

        public bool IsTFH1 => SelectedTimeFrame == TF.Hour1;
        public bool IsTFH4 => SelectedTimeFrame == TF.Hour4;
        public bool IsTFD1 => SelectedTimeFrame == TF.Day1;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsLongSelected))]
        [NotifyPropertyChangedFor(nameof(IsShortSelected))]
        private TD selectedTradeDirection;

        public bool IsLongSelected => SelectedTradeDirection == TD.Long;
        public bool IsShortSelected => SelectedTradeDirection == TD.Short;

        public ObservableCollection<Option<TF>> TimeFrameChoices { get; } =
            new(Enum.GetValues(typeof(TF)).Cast<TF>()
                .Select(tf => new Option<TF>(tf, DisplayText.TimeFrameLabel(tf))));

        [ObservableProperty] private Option<TF>? selectedTimeFrameOption;

        public ObservableCollection<Preset> Presets { get; private set; } = new();
        [ObservableProperty] private Preset? selectedPreset;

        [ObservableProperty] private bool isTechnicalSelected;
        [ObservableProperty] private bool isPatternSelected;
        [ObservableProperty] private bool isAiSelected;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreview))]
        private string? previewImagePath;

        public bool HasPreview => !string.IsNullOrWhiteSpace(PreviewImagePath);
        #endregion

        #region Commands
        public IRelayCommand<string> SetMarketTypeFromStringCommand { get; }
        public IRelayCommand<string> SetTimeFrameFromStringCommand { get; }
        public IRelayCommand<string> SetTradeDirectionFromStringCommand { get; }
        public IRelayCommand<string> ToggleStrategyFocusCommand { get; }
        public IAsyncRelayCommand UploadImageCommand { get; }
        public IAsyncRelayCommand AnalyzeChartCommand { get; }
        public IAsyncRelayCommand CancelCommand { get; }
        #endregion

        #region Ctor
        public NewPredictionVM()
        {
            SetMarketTypeFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<MT>(s, out var mt)) SelectedMarketType = mt;
            });

            SetTimeFrameFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<TF>(s, out var tf))
                {
                    SelectedTimeFrame = tf;
                    SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(tf));
                }
            });

            SetTradeDirectionFromStringCommand = new RelayCommand<string>(s =>
            {
                if (Enum.TryParse<TD>(s, out var td)) SelectedTradeDirection = td;
            });

            ToggleStrategyFocusCommand = new RelayCommand<string>(key =>
            {
                switch (key)
                {
                    case "Technical": IsTechnicalSelected = !IsTechnicalSelected; break;
                    case "Pattern": IsPatternSelected = !IsPatternSelected; break;
                    case "AI": IsAiSelected = !IsAiSelected; break;
                }
            });

            UploadImageCommand = new AsyncRelayCommand(OnUploadTappedAsync);
            AnalyzeChartCommand = new AsyncRelayCommand(OnAnalyzeChartAsync);
            CancelCommand = new AsyncRelayCommand(ResetFiltersAsync);
        }
        #endregion

        #region Partial Change Handlers
        partial void OnSelectedMarketTypeChanged(MT value)
        {
            AnalysisSession.MarketType = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTimeFrameChanged(TF value)
        {
            AnalysisSession.TimeFrame = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTradeDirectionChanged(TD value)
        {
            AnalysisSession.TradeDirection = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }

        partial void OnSelectedTimeFrameOptionChanged(Option<TF>? value)
        {
            if (value != null) SelectedTimeFrame = value.Value;
        }

        partial void OnSelectedPresetChanged(Preset? value)
        {
            AnalysisSession.Preset = value;

            if (value?.MarketType.HasValue == true)
                SelectedMarketType = value.MarketType.Value;

            if (value?.TimeFrame.HasValue == true)
            {
                SelectedTimeFrame = value.TimeFrame.Value;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));
            }

            if (value?.TradeDirection.HasValue == true)
                SelectedTradeDirection = value.TradeDirection.Value;
        }

        partial void OnPreviewImagePathChanged(string? value)
        {
            AnalysisSession.PreviewImage = value;
            OnPropertyChanged(nameof(AnalysisSession));
        }
        #endregion

        #region Methods
        private void LoadPresetsFromFaker(int count = 10)
        {
            var list = PresetFaker.CreateList(count);
            Presets = new ObservableCollection<Preset>(list);
            OnPropertyChanged(nameof(Presets));
        }
        #endregion

        #region Tasks
        public Task InitializeAsync()
        {
            SelectedMarketType = MT.Crypto;
            SelectedTimeFrame = TF.Hour1;
            SelectedTradeDirection = TD.Long;
            SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(SelectedTimeFrame));

            IsTechnicalSelected = true;
            IsPatternSelected = false;
            IsAiSelected = false;

            LoadPresetsFromFaker(10);

            SelectedPreset = Presets.FirstOrDefault(p => p.MarketType == SelectedMarketType)
                             ?? Presets.FirstOrDefault();

            AnalysisSession.CreatedAt = DateTime.Now;
            AnalysisSession.Preset = SelectedPreset;
            PreviewImagePath = null;
            return Task.CompletedTask;
        }

        private async Task ResetFiltersAsync()
        {
            await InitializeAsync();
        }

        private async Task OnUploadTappedAsync()
        {
            var choice = await Shell.Current.DisplayActionSheet("Upload chart", "Cancel", null, "Take Photo", "Choose from Gallery");
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;

            try
            {
                FileResult? result = null;
                var isVirtual = DeviceInfo.Current.DeviceType == DeviceType.Virtual;

                if (choice == "Take Photo")
                {
                    if (!await EnsureCameraPermissionAsync())
                    {
                        await ShowPopupAsync("Camera permission is required.");
                        return;
                    }

                    if (isVirtual || !MediaPicker.Default.IsCaptureSupported)
                    {
                        await ShowPopupAsync(isVirtual
                            ? "Emulator/simulator camera not available. Opening gallery instead."
                            : "Camera not available. Opening gallery instead.");

                        if (!await EnsureGalleryPermissionAsync()) { await ShowPopupAsync("Gallery permission denied."); return; }
                        result = await MediaPicker.Default.PickPhotoAsync();
                    }
                    else
                    {
                        result = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions { Title = "Chart photo" });
                    }
                }
                else
                {
                    if (!await EnsureGalleryPermissionAsync()) { await ShowPopupAsync("Gallery permission denied."); return; }
                    result = await MediaPicker.Default.PickPhotoAsync();
                }

                if (result == null) return;

                var targetDir = FileSystem.AppDataDirectory;
                var ext = Path.GetExtension(result.FileName);
                var fileName = $"chart_{DateTime.UtcNow:yyyyMMdd_HHmmss}{ext}";
                var destPath = Path.Combine(targetDir, fileName);

                using var src = await result.OpenReadAsync();
                using var dst = File.Create(destPath);
                await src.CopyToAsync(dst);

                PreviewImagePath = destPath;
            }
            catch (FeatureNotSupportedException)
            {
                if (await EnsureGalleryPermissionAsync())
                {
                    var r = await MediaPicker.Default.PickPhotoAsync();
                    if (r != null)
                    {
                        var targetDir = FileSystem.AppDataDirectory;
                        var ext = Path.GetExtension(r.FileName);
                        var file = $"chart_{DateTime.UtcNow:yyyyMMdd_HHmmss}{ext}";
                        var dest = Path.Combine(targetDir, file);
                        using var src = await r.OpenReadAsync();
                        using var dst = File.Create(dest);
                        await src.CopyToAsync(dst);
                        PreviewImagePath = dest;
                        return;
                    }
                }
                await ShowPopupAsync("This emulator doesn’t support camera/photo picker. Try a Google Play AVD or a real device.");
            }
            catch (PermissionException)
            {
                await ShowPopupAsync("Permission denied. Enable it in Settings and try again.");
            }
            catch
            {
                await ShowPopupAsync("Failed to get image. Please try again.");
            }
        }

        private async Task<bool> EnsureCameraPermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.Camera>();
            return status == PermissionStatus.Granted;
        }

        private async Task<bool> EnsureGalleryPermissionAsync()
        {
            if (DeviceInfo.Current.Platform == DevicePlatform.Android && DeviceInfo.Current.Version.Major < 13)
            {
                var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                    status = await Permissions.RequestAsync<Permissions.StorageRead>();
                return status == PermissionStatus.Granted;
            }
            return true;
        }

        private async Task OnAnalyzeChartAsync()
        {
            if (string.IsNullOrWhiteSpace(PreviewImagePath))
            {
                await ShowPopupAsync("Please upload a chart image before analyzing.");
                return;
            }

            AnalysisSession.CreatedAt = DateTime.Now;
            AnalysisSession.Preset = SelectedPreset;

            var mt = DisplayText.Market(SelectedMarketType);
            var tf = DisplayText.TimeFrameLabel(SelectedTimeFrame);
            var dir = DisplayText.Direction(SelectedTradeDirection);
            var presetName = SelectedPreset?.Name ?? "Recommended";

            var msg = $"Market: {mt}\nTimeframe: {tf}\nDirection: {dir}\nPreset: {presetName}";
            await Shell.Current.DisplayAlert("Info", msg, "OK");
        }

        private async Task<object?> ShowPopupAsync(string message) => await Shell.Current.ShowPopupAsync(new InfoPopup(message));
        #endregion
    }
}

// To import presets from your online repo later, replace the body of LoadPresetsFromFaker(int) with your fetch logic and still assign Presets + OnPropertyChanged(nameof(Presets)), then keep InitializeAsync() calling that method.
