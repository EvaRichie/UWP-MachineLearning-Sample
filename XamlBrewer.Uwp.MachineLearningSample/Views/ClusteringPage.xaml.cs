﻿using Mvvm.Services;
using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using XamlBrewer.Uwp.MachineLearningSample.Models;
using XamlBrewer.Uwp.MachineLearningSample.ViewModels;

namespace XamlBrewer.Uwp.MachineLearningSample
{
    public sealed partial class ClusteringPage : Page
    {
        private List<OxyColor> _colors = new List<OxyColor>
                {
                    OxyColors.Black,
                    OxyColors.LightCoral,
                    OxyColors.Khaki,
                    OxyColors.SlateBlue,
                    OxyColors.DarkCyan,
                    OxyColors.LightSkyBlue,
                    OxyColors.HotPink
                };

        public ClusteringPage()
        {
            this.InitializeComponent();
            this.DataContext = new ClusteringPageViewModel();

            Loaded += Page_Loaded;
        }

        private ClusteringPageViewModel ViewModel => DataContext as ClusteringPageViewModel;

        private async void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DatasetBox.IsChecked = false;
            SettingUpBox.IsChecked = false;
            TrainingBox.IsChecked = false;
            CalculatingBox.IsChecked = false;
            PlottingBox.IsChecked = false;
            PrepareDiagram();

            // Preparing the files.
            DatasetBox.IsChecked = true;
            var trainingDataPath = await MlDotNet.FilePath(@"ms-appx:///Data/Mall_Customers.csv");

            // Read training data.
            var trainingDataView = await ViewModel.Load(trainingDataPath);

            // Create the model.
            SettingUpBox.IsChecked = true;
            await ViewModel.Build();

            // Train the model.    
            TrainingBox.IsChecked = true;
            await ViewModel.Train(trainingDataView);

            // Save the model.
            await ViewModel.Save("clusteringModel.zip");

            // Run the model on a set of data.
            CalculatingBox.IsChecked = true;
            var predictions = await ViewModel.Predict(trainingDataView);

            // Draw the results.
            PlottingBox.IsChecked = true;
            foreach (var prediction in predictions)
            {
                Diagram.Model.Annotations.Add(
                    new PointAnnotation
                    {
                        Shape = MarkerType.Circle,
                        X = prediction.SpendingScore,
                        Y = prediction.AnnualIncome,
                        Fill = _colors[(int)prediction.PredictedCluster]
                    });
            }

            Diagram.InvalidatePlot();
        }

        private void PrepareDiagram()
        {
            var foreground = OxyColors.LightSteelBlue;
            var plotModel = new PlotModel
            {
                PlotAreaBorderThickness = new OxyThickness(1, 0, 0, 1),
                PlotAreaBorderColor = foreground
            };

            var linearAxisX = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Spending Score",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };

            plotModel.Axes.Add(linearAxisX);
            var linearAxisY = new LinearAxis
            {
                Maximum = 140,
                Title = "Annual Income",
                TextColor = foreground,
                TicklineColor = foreground,
                TitleColor = foreground
            };
            plotModel.Axes.Add(linearAxisY);
            Diagram.Model = plotModel;
            linearAxisY.Reset();
        }

        private async void Calculate_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            int.TryParse(AnnualIncomeInput.Text, out int annualIncome);
            int.TryParse(SpendingScoreInput.Text, out int spendingScore);
            var output = await ViewModel.Predict(new ClusteringData { AnnualIncome = annualIncome, SpendingScore = spendingScore });
            var annotation = new PointAnnotation { Shape = MarkerType.Diamond, X = output.SpendingScore, Y = output.AnnualIncome, Fill = _colors[(int)output.PredictedCluster], TextColor = OxyColors.LightSteelBlue, Text = "Here" };
            Diagram.Model.Annotations.Add(annotation);
            Diagram.InvalidatePlot();
        }
    }
}
