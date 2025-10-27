using ChartSightAI.MVVM.Models;
using ChartSightAI.Popups;
using ChartSightAI.Utility;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
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
using System.Diagnostics;
using ChartSightAI.Services;
using ChartSightAI.Services.Repos;
// keep if you use those elsewhere
using ChartSightAI.DTO_S.AI_S;
using System.Text.Json;

namespace ChartSightAI.MVVM.ViewModels
{
    public partial class NewPredictionVM : ObservableObject
    {
        private readonly AuthService _auth;
        private readonly PresetRepo _presetRepo;
        private readonly AnalysisSessionRepo _analysisSession;
        private AssistantClient _assistant;

        public NewPredictionVM(AuthService authService, PresetRepo presetRepo, AnalysisSessionRepo analysisSessionRepo)
        {
            _auth = authService;
            _presetRepo = presetRepo;
            _analysisSession = analysisSessionRepo; // <- FIX: assign or InsertAsync will null-ref
        }

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

        [ObservableProperty] private ObservableCollection<Preset> _presets = new();
        [ObservableProperty] private Preset? selectedPreset;

        [ObservableProperty] private bool isTechnicalSelected;
        [ObservableProperty] private bool isPatternSelected;
        [ObservableProperty] private bool isAiSelected;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasPreview))]
        private string? previewImagePath;

        public bool HasPreview => !string.IsNullOrWhiteSpace(PreviewImagePath);

        [ObservableProperty] private bool isAiSheetOpen;
        [ObservableProperty] private AiAnalysisResult? analysis;
        [ObservableProperty] private bool _isAnalyzing;
        #endregion

        #region Commands
        public IRelayCommand<string> SetMarketTypeFromStringCommand => new RelayCommand<string>(s =>
        {
            if (Enum.TryParse<MT>(s, out var mt)) SelectedMarketType = mt;
        });

        public IRelayCommand<string> SetTimeFrameFromStringCommand => new RelayCommand<string>(s =>
        {
            if (Enum.TryParse<TF>(s, out var tf))
            {
                SelectedTimeFrame = tf;
                SelectedTimeFrameOption = TimeFrameChoices.FirstOrDefault(o => o.Value.Equals(tf));
            }
        });

        public IRelayCommand<string> SetTradeDirectionFromStringCommand => new RelayCommand<string>(s =>
        {
            if (Enum.TryParse<TD>(s, out var td)) SelectedTradeDirection = td;
        });

        public IRelayCommand<string> ToggleStrategyFocusCommand => new RelayCommand<string>(key =>
        {
            switch (key)
            {
                case "Technical": IsTechnicalSelected = !IsTechnicalSelected; break;
                case "Pattern": IsPatternSelected = !IsPatternSelected; break;
                case "AI": IsAiSelected = !IsAiSelected; break;
            }
        });

        public IAsyncRelayCommand UploadImageCommand => new AsyncRelayCommand(OnUploadTappedAsync);
        public IAsyncRelayCommand AnalyzeChartCommand => new AsyncRelayCommand(OnAnalyzeChartAsync);
        public IAsyncRelayCommand CancelCommand => new AsyncRelayCommand(ResetFiltersAsync);
        public IRelayCommand CloseAiSheetCommand => new RelayCommand(() => IsAiSheetOpen = false);
        public IAsyncRelayCommand SaveAnalysisCommand => new AsyncRelayCommand(SaveAnalysisAsync);
        public IAsyncRelayCommand ShareAnalysisCommand => new AsyncRelayCommand(ShareAnalysisAsync);
        #endregion

        #region Methods (property change hooks)
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

        #region Save / Share
        private async Task SaveAnalysisAsync()
        {
            try
            {
                await _auth.InitializeAsync();
                var uid = await _auth.GetUserIdAsync();
                if (uid is null)
                {
                    await ShowPopupAsync("Please log in to save analysis.");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                if (Analysis == null)
                {
                    await ShowPopupAsync("Nothing to save yet. Run Analyze first.");
                    return;
                }

               
                var normalizedJson = Newtonsoft.Json.JsonConvert.SerializeObject(
                    Analysis,
                    new Newtonsoft.Json.JsonSerializerSettings { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore }
                );
                var normalized = Newtonsoft.Json.JsonConvert.DeserializeObject<AiAnalysisResult>(normalizedJson);

                var s = new AnalysisSession
                {
                    MarketType = SelectedMarketType,
                    TimeFrame = SelectedTimeFrame,
                    Preset = SelectedPreset,
                    TradeDirection = SelectedTradeDirection,
                    CreatedAt = DateTime.Now,
                    Result = normalized,           // use the normalized object
                    PreviewImage = PreviewImagePath
                };

                await _analysisSession.InsertAsync(uid.Value, s);
                System.Diagnostics.Debug.WriteLine("Analysis saved.");
                await ShowPopupAsync("Analysis saved.");
            }
            catch (Exception ex)
            {
                await ShowPopupAsync($"Save failed: {ex.Message}");
            }
        }


        private async Task ShareAnalysisAsync()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("ChartSightAI Analysis");
            sb.AppendLine($"{SelectedMarketType} • {SelectedTimeFrame} • {SelectedTradeDirection}");
            if (Analysis != null)
            {
                if (!string.IsNullOrWhiteSpace(Analysis.Summary)) sb.AppendLine().AppendLine($"Summary: {Analysis.Summary}");
                if (!string.IsNullOrWhiteSpace(Analysis.TrendAnalysis)) sb.AppendLine().AppendLine($"Trend: {Analysis.TrendAnalysis}");
                if (!string.IsNullOrWhiteSpace(Analysis.Pattern)) sb.AppendLine().AppendLine($"Pattern: {Analysis.Pattern}");
                if (!string.IsNullOrWhiteSpace(Analysis.Explainability))
                    sb.AppendLine().AppendLine("Explainability: " + Analysis.Explainability);
                if (Analysis.TradeIdea != null)
                {
                    sb.AppendLine().AppendLine("Trade Idea:");
                    sb.AppendLine($"Entry: {Analysis.TradeIdea.Entry:F2}");
                    sb.AppendLine($"Stop: {Analysis.TradeIdea.StopLoss:F2}");
                    if (Analysis.TradeIdea.Targets?.Count > 0)
                        sb.AppendLine($"Targets: {string.Join(", ", Analysis.TradeIdea.Targets.Select(t => t.ToString("F2")))}");
                }
            }
            await Share.Default.RequestAsync(
                new ShareTextRequest
                {
                    Title = "ChartSightAI Analysis",
                    Text = sb.ToString()
                });
        }
        #endregion

        #region Init / Reset
        public async Task InitializeAsync()
        {
            try
            {
                await _auth.InitializeAsync();

                IsTechnicalSelected = true;
                IsPatternSelected = false;
                IsAiSelected = false;

                Presets.Clear();

                var uid = await _auth.GetUserIdAsync();
                if (uid is null)
                {
                    await ShowPopupAsync("Please log in to load presets.");
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var list = await _presetRepo.GetAllAsync(uid.Value);
                foreach (var item in list)
                    Presets.Add(item);

                SelectedPreset = Presets.FirstOrDefault(p => p.MarketType == SelectedMarketType)
                                 ?? Presets.FirstOrDefault();

                AnalysisSession.CreatedAt = DateTime.Now;
                AnalysisSession.Preset = SelectedPreset;
                PreviewImagePath = null;
                Analysis = null;
                IsAiSheetOpen = false;
            }
            catch (Exception ex)
            {
                await ShowPopupAsync($"Init failed: {ex.Message}");
            }
        }

        private Task ResetFiltersAsync() => InitializeAsync();
        #endregion

        #region Image / Permissions / AI
        public async Task ShowPopupAsync(string msg)
        {
            await Shell.Current.ShowPopupAsync(new InfoPopup(msg));
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
            try
            {
                if (_assistant == null)
                    _assistant = await AssistantClient.CreateFromAssetAsync();

                IsAnalyzing = true;

                var req = new AnalysisRequest
                {
                    MarketType = SelectedMarketType,
                    TimeFrame = SelectedTimeFrame,
                    TradeDirection = SelectedTradeDirection,
                    // ⬇️ Always source indicators from helper (AI output ignored later)
                    Indicators = MarketIndicatorHelper.GetDefaultIndicators(SelectedMarketType),
                    PreviewImage = PreviewImagePath,
                    Notes = SelectedPreset?.Description
                };

                var result = await _assistant.AnalyzeAsync(req);

                if (result == null)
                {
                    await ShowPopupAsync("No analysis returned. Please try again.");
                    return;
                }

                var explicitNotChart =
                    result.Summary?.Trim().Equals("Image is not a trading price chart.", StringComparison.OrdinalIgnoreCase) == true;

                if (explicitNotChart)
                {
                    await ShowPopupAsync("This image doesn’t look like a price chart. Please upload a trading chart (candles/ohlc/line).");
                    return;
                }

                Analysis = result;
                IsAiSheetOpen = true;
            }
            catch (JsonException jx)
            {
                var at = string.IsNullOrWhiteSpace(jx.Path) ? "$" : jx.Path;
                await ShowPopupAsync($"Analysis failed: JSON invalid at {at}. {jx.Message}");
            }
            catch (Exception ex)
            {
                await ShowPopupAsync($"Analysis failed: {ex.Message}");
            }
            finally
            {
                IsAnalyzing = false;
            }
        }


        #endregion
    }
}
