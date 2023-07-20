	#region Using declarations
	using System;
	using System.IO;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.ComponentModel.DataAnnotations;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows;
	using System.Windows.Input;
	using System.Windows.Media;
	using System.Xml.Serialization;
	using System.Globalization;
	using NinjaTrader.Cbi;
	using NinjaTrader.Gui;
	using NinjaTrader.Gui.Chart;
	using NinjaTrader.Gui.SuperDom;
	using NinjaTrader.Gui.Tools;
	using NinjaTrader.Data;
	using NinjaTrader.NinjaScript;
	using NinjaTrader.Core.FloatingPoint;
	using NinjaTrader.NinjaScript.DrawingTools;
	// Manually added:
	using NinjaTrader.NinjaScript.AddOns;
	using System.IO;
	#endregion

	//This namespace holds Indicators in this folder and is required. Do not change it. 
	namespace NinjaTrader.NinjaScript.Indicators
	{
		
		public class DailyPriceLevels : Indicator
		{
			#region Variables
			private Dictionary<DateTime, List<double>> _levelsByDate;
			private DateTime _lastFileModifiedDate;
			private long _lastMaxLevels;
			private static string version = "1.3.1";
			#endregion
			
			private void readCSV(string filename)
			{
				/* File Format
				date | levels
				2023-07-03 | p1, p2, p3, p4
				2023-07-05 | p1, p2, p3, p5, p6, p7
				*/
				if (!File.Exists(filename))
				{
					Log("File " + LevelFileName + " not found", LogLevel.Error);
					return;
				}

				var fileModifiedDate = File.GetLastWriteTime(filename);
				if(fileModifiedDate == _lastFileModifiedDate)
				{
					return;;
				}
				_lastFileModifiedDate = fileModifiedDate;
				_levelsByDate = new Dictionary<DateTime, List<double>>();	
				long maxLevels = 0;
				using(StreamReader reader = new StreamReader(filename))
				{
					string line = reader.ReadLine();
					// skip header
					line = reader.ReadLine();
					while (line != null) 
					{
						string[] fields = line.Split('|');
						CultureInfo culture = new CultureInfo("en-US");
						DateTime date = DateTime.ParseExact(fields[0].Trim(), "yyyy-MM-dd", culture);
						_levelsByDate.Add(date.Date, new List<Double>());
						string [] levels =  fields[1].Split(',');
						maxLevels = Math.Max(levels.Length, maxLevels);
						foreach(string level in levels)
						{				
							_levelsByDate[date.Date].Add(double.Parse(level));
						}
						line = reader.ReadLine();
					}
				}
				Log(Name + " read file " + LevelFileName + " with modified date of " + _lastFileModifiedDate + " and maxLevels " + maxLevels, LogLevel.Information);
			}

			protected override void OnStateChange()
			{
				if (State == State.SetDefaults)
				{
					Description									= @"Draws prices levels as an time series indicator values";
					Name										= "Daily Price Levels Cargocult";
					Calculate									= Calculate.OnBarClose;
					IsOverlay									= true;
					DisplayInDataBox							= false;
					DrawOnPricePanel							= true;
					DrawHorizontalGridLines						= true;
					DrawVerticalGridLines						= true;
					PaintPriceMarkers							= true;
					ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
					//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
					//See Help Guide for additional information.
					IsSuspendedWhileInactive					= true;
					LevelFileName					= string.Empty;
					LevelLineColor = Brushes.MediumVioletRed;
					LevelLineWidth = 2;
					LevelLineOpacity = 50;
					LevelLineType = DashStyleHelper.Dot;
					LevelScaleFactor = 1.0;
					Log("Daily Price Levels by Cargocult version " + version, LogLevel.Information);
				}
				else if (State == State.Configure)
				{
					readCSV(LevelFileName);
					long maxNumberOfLevels = 25;
					foreach(KeyValuePair<DateTime, List<double>> entry in _levelsByDate)
					{
						maxNumberOfLevels = Math.Max(maxNumberOfLevels, entry.Value.Count);
					}
				
					for(int i=0; i < maxNumberOfLevels; ++i) 
					{
						AddPlot(new Stroke(LevelLineColor, LevelLineType, LevelLineWidth, LevelLineOpacity), PlotStyle.HLine, "level_" + i);
					}
				}
			}

			protected override void OnBarUpdate()
			{
				readCSV(LevelFileName);
				if (_levelsByDate == null) return;
				if(_levelsByDate.ContainsKey(Time[0].Date)) 
				{
					var levels = _levelsByDate[Time[0].Date];
					int counter = 0;
					foreach(double level in levels)
					{
						if(counter >= Values.Length) 
						{
							Log("More levels added than plots - please refresh your chart using F5", LogLevel.Error);
							return;
						}
						Values[counter++][0] = level * LevelScaleFactor;
					}
				}
			}
			
			#region Properties
			
			[NinjaScriptProperty]
			[Display(Name="Level FileName", Description="Name of the csv file containing the price levels", Order=1, GroupName="Level Parameters")]
			public string LevelFileName
			{ get; set; }
			
			[XmlIgnore()]
			[Display(Name = "Level Line Color", Description = "Color of level line", Order = 3, GroupName="Level Parameters")]
			public Brush LevelLineColor
			{ get; set; }
					 
			[Browsable(false)]
			public string LineColorSerialize
			{
				get { return Serialize.BrushToString(LevelLineColor); }
	   			set { LevelLineColor = Serialize.StringToBrush(value); }
			}
			
			[Range(1, int.MaxValue)]
			[NinjaScriptProperty]
			[Display(Name="Level Line Width", Description="Width of level line", Order=4, GroupName="Level Parameters")]
			public int LevelLineWidth
			{ get; set; }

			[Range(1, 100)]
			[NinjaScriptProperty]
			[Display(Name="Level Line Opacity", Description="Opacity of level line", Order=5, GroupName="Level Parameters")]
			public int LevelLineOpacity
			{ get; set; }

			[Display(Name="Level Line Type", Description="Type of line (line, dot, dash, etc.)", Order=6, GroupName="Level Parameters")]
			public DashStyleHelper LevelLineType
			{ get; set; }

			[Range(0, double.MaxValue)]
			[NinjaScriptProperty]
			[Display(Name="Level Scale Factor", Description="Scale Factor For Level Values", Order=7, GroupName="Level Parameters")]
			public double LevelScaleFactor
			{ get; set; }

			#endregion

		}
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DailyPriceLevels[] cacheDailyPriceLevels;
		public DailyPriceLevels DailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			return DailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity, levelScaleFactor);
		}

		public DailyPriceLevels DailyPriceLevels(ISeries<double> input, string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			if (cacheDailyPriceLevels != null)
				for (int idx = 0; idx < cacheDailyPriceLevels.Length; idx++)
					if (cacheDailyPriceLevels[idx] != null && cacheDailyPriceLevels[idx].LevelFileName == levelFileName && cacheDailyPriceLevels[idx].LevelLineWidth == levelLineWidth && cacheDailyPriceLevels[idx].LevelLineOpacity == levelLineOpacity && cacheDailyPriceLevels[idx].LevelScaleFactor == levelScaleFactor && cacheDailyPriceLevels[idx].EqualsInput(input))
						return cacheDailyPriceLevels[idx];
			return CacheIndicator<DailyPriceLevels>(new DailyPriceLevels(){ LevelFileName = levelFileName, LevelLineWidth = levelLineWidth, LevelLineOpacity = levelLineOpacity, LevelScaleFactor = levelScaleFactor }, input, ref cacheDailyPriceLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DailyPriceLevels DailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			return indicator.DailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity, levelScaleFactor);
		}

		public Indicators.DailyPriceLevels DailyPriceLevels(ISeries<double> input , string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			return indicator.DailyPriceLevels(input, levelFileName, levelLineWidth, levelLineOpacity, levelScaleFactor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DailyPriceLevels DailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			return indicator.DailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity, levelScaleFactor);
		}

		public Indicators.DailyPriceLevels DailyPriceLevels(ISeries<double> input , string levelFileName, int levelLineWidth, int levelLineOpacity, double levelScaleFactor)
		{
			return indicator.DailyPriceLevels(input, levelFileName, levelLineWidth, levelLineOpacity, levelScaleFactor);
		}
	}
}

#endregion
