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
	using System.Linq;
	// Manually added:
	using System.IO;
	#endregion
	
	//This namespace holds Indicators in this folder and is required. Do not change it. 
	namespace NinjaTrader.NinjaScript.Indicators
	{
		
		public class CargocultDailyPriceLevels : Indicator
		{
			#region Variables
			private SortedDictionary<DateTime, List<double>> _levelsByDate;
			private Dictionary<DateTime, KeyValuePair<DateTime, List<double>>> _chartDateToLevelsDateCache;
			private DateTime _lastFileModifiedDate;
			private long _lastMaxLevels;
			private static string version = "1.6.0";
			private Series<double> currentUpperLevel;
			private Series<double> currentLowerLevel;
			#endregion
			
			private void resetCache()
			{
				Log("Resetting chart date to level date cache", LogLevel.Information);
				_chartDateToLevelsDateCache = new Dictionary<DateTime, KeyValuePair<DateTime, List<double>>>();
			}
			private void readCSV(string filename)
			{
				/* 
				::Filename format - Indicator will replace the text <symbol> with whatever the base symbol name is (eg spy, es, nq, meq, mes)
				
				example "<symbol>_levels.csv" will become es_levels.csv and will look for this file 
				
				File Format
				date | levels
				2023-07-03 | p1, p2, p3, p4
				2023-07-05 | p1, p2, p3, p5, p6, p7
				
				*/
				string resolved_filename = filename.Replace("<symbol>", Instrument.MasterInstrument.Name.ToLower());
				if (!File.Exists(resolved_filename))
				{
					Log("File " + resolved_filename + " not found", LogLevel.Error);
					return;
				}

				var fileModifiedDate = File.GetLastWriteTime(resolved_filename);
				if(fileModifiedDate == _lastFileModifiedDate)
				{
					return;;
				}
				_lastFileModifiedDate = fileModifiedDate;
				_levelsByDate = new SortedDictionary<DateTime, List<double>>();	
				// reset cache as mapping may be invalid with new file
				resetCache();
				long maxLevels = 0;
				using(StreamReader reader = new StreamReader(resolved_filename))
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
							_levelsByDate[date.Date].Add(double.Parse(level, culture));
						}
						
						line = reader.ReadLine();
					}
				}
				Log(Name + " read file " + resolved_filename + " with modified date of " + _lastFileModifiedDate + " and maxLevels " + maxLevels, LogLevel.Information);
			}

			protected override void OnStateChange()
			{
				if (State == State.SetDefaults)
				{
					Description									= @"Draws prices levels as an time series indicator values";
					Name										= "Cargocult Daily Price Levels";
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
					MaxDayCarryForward = 1;
					VerboseLogging = false;
					// todo - cargo - figure out if this is the best place for a reset of cache
					resetCache();
					Log("Daily Price Levels by Cargocult version " + version, LogLevel.Information);
				}
				else if (State == State.Configure)
				{
					currentUpperLevel 			= new Series<double>(this);
					currentLowerLevel			= new Series<double>(this);
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

			private KeyValuePair<DateTime, List<double>> getLevelsForDate()
			{
				var currentDate = Time[0].Date;
				if(_chartDateToLevelsDateCache.ContainsKey(currentDate))
				{
					return _chartDateToLevelsDateCache[currentDate];
				}
				else 
				{
					var kv = _levelsByDate.LastOrDefault(x => ( x.Key <= currentDate && (currentDate - x.Key).Days <= MaxDayCarryForward) );
					_chartDateToLevelsDateCache[currentDate] = kv;
					var levelsFileDate = kv.Key;
					var logLevel = levelsFileDate == currentDate ? LogLevel.Information : levelsFileDate < currentDate ? LogLevel.Warning : LogLevel.Error;
					Log(String.Format("Using file level date: {0} for chart date: {1} maxDayCarryFoward: {2} symbol: {3}", 
						levelsFileDate, currentDate, MaxDayCarryForward, Instrument.MasterInstrument.Name.ToLower()), logLevel);
					return kv;
				}
			}
				
			private bool isLevelKVValid(KeyValuePair<DateTime, List<double>> levelsKV)
			{
				return !levelsKV.Equals(default(KeyValuePair<DateTime, List<double>>));
			}
			
			protected override void OnBarUpdate()
			{
				readCSV(LevelFileName);
				if (_levelsByDate == null) return;
				
				var currentDate = Time[0].Date;
				
				var levelsKV = getLevelsForDate();
				
				if(!isLevelKVValid(levelsKV))
				{
					if(VerboseLogging) 
					{
						// TODO cargo - once caching is implemented, always log this
						Log(String.Format("No file date for chart date: {0} maxDayCarryForward {1} symbol: {2}", 
						currentDate, MaxDayCarryForward, Instrument.MasterInstrument.Name.ToLower()), LogLevel.Error);
					}
				}
				else
				{
					double input0 = Input[0];
					double upper_level = double.NaN;
					double lower_level = double.NaN;
					double closest_level = double.NaN;
					int counter = 0;
					var levelsFileDate = levelsKV.Key;
					foreach(double level in levelsKV.Value)
					{
						// TODO cargo - once caching is implemented, always log this
						if(VerboseLogging) 
						{
							var logLevel = levelsFileDate == currentDate ? LogLevel.Information : levelsFileDate < currentDate ? LogLevel.Warning : LogLevel.Error;
							Log(String.Format("Using file level date: {0} for chart date: {1} maxDayCarryFoward: {2} symbol: {3}", 
								levelsFileDate, currentDate, MaxDayCarryForward, Instrument.MasterInstrument.Name.ToLower()), logLevel);
						}
		
						if(counter >= Values.Length) 
						{
							Log("More levels added than plots - please refresh your chart using F5", LogLevel.Error);
							return;
						}
						Values[counter++][0] = level;
						if(level >= input0) 
						{
							if(double.IsNaN(upper_level) || level < upper_level)
							{
								upper_level = level;
							}
						}
						if(level <= input0) 
						{
							if(double.IsNaN(lower_level) || level > lower_level)
							{
								lower_level = level;
							}
						}
						if(double.IsNaN(closest_level) || (Math.Abs(level-input0) < Math.Abs(closest_level-input0)))
						{
							closest_level = level;
						}
					}
					currentUpperLevel[0] = upper_level;
					currentLowerLevel[0] = lower_level;
				}
			}
			
			#region Properties
			
			[Browsable(false)]
			[XmlIgnore]		
        	public Series <double> CurrentUpperLevel
        	{
            	get { return currentUpperLevel; }	
        	}

			[Browsable(false)]
			[XmlIgnore]		
        	public Series <double> CurrentLowerLevel
        	{
            	get { return currentLowerLevel; }	
        	}

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
			
			[Display(Name="Max Day Carry Forward", Description="Use the most recent date's data if current date less than or equal to this many days ahead in time", Order=7, GroupName="Level Parameters")]
			[Range(0, 10000)]
			public int MaxDayCarryForward
			{ get; set; }

			[Display(Name="Verbose Logging", Description="Log more info about what the indicator is doing", Order=1, GroupName="Misc")]
			public bool VerboseLogging
			{ get; set; }

			#endregion

		}
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultDailyPriceLevels[] cacheCargocultDailyPriceLevels;
		public CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			return CargocultDailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity);
		}

		public CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input, string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			if (cacheCargocultDailyPriceLevels != null)
				for (int idx = 0; idx < cacheCargocultDailyPriceLevels.Length; idx++)
					if (cacheCargocultDailyPriceLevels[idx] != null && cacheCargocultDailyPriceLevels[idx].LevelFileName == levelFileName && cacheCargocultDailyPriceLevels[idx].LevelLineWidth == levelLineWidth && cacheCargocultDailyPriceLevels[idx].LevelLineOpacity == levelLineOpacity && cacheCargocultDailyPriceLevels[idx].EqualsInput(input))
						return cacheCargocultDailyPriceLevels[idx];
			return CacheIndicator<CargocultDailyPriceLevels>(new CargocultDailyPriceLevels(){ LevelFileName = levelFileName, LevelLineWidth = levelLineWidth, LevelLineOpacity = levelLineOpacity }, input, ref cacheCargocultDailyPriceLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			return indicator.CargocultDailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity);
		}

		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input , string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			return indicator.CargocultDailyPriceLevels(input, levelFileName, levelLineWidth, levelLineOpacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			return indicator.CargocultDailyPriceLevels(Input, levelFileName, levelLineWidth, levelLineOpacity);
		}

		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input , string levelFileName, int levelLineWidth, int levelLineOpacity)
		{
			return indicator.CargocultDailyPriceLevels(input, levelFileName, levelLineWidth, levelLineOpacity);
		}
	}
}

#endregion
