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
		// the bool is true of the level is not for the given day
		// TODO replace bool with an enum or something self descriptive
		using Levels = Dictionary<double, bool>;
		using DateLevelsPair = KeyValuePair<DateTime, Dictionary<double, bool>>;
		using LevelsDictionary = SortedDictionary<DateTime, Dictionary<double, bool>>;
		using ChartDateToLevelsDateDictionary = Dictionary<DateTime, KeyValuePair<DateTime, Dictionary<double, bool>>>;
		
		public class CargocultDailyPriceLevels : Indicator
		{
			#region Variables
			
			private LevelsDictionary _levelsByDate;
			private ChartDateToLevelsDateDictionary _chartDateToLevelsDateCache;
			private DateTime _lastFileModifiedDate;
			private static string version = "1.8.0";
			private Series<double> currentUpperLevel;
			private Series<double> currentLowerLevel;
			#endregion
			
			private void resetCache()
			{
				Log("Resetting chart date to level date cache", LogLevel.Information);
				_chartDateToLevelsDateCache = new ChartDateToLevelsDateDictionary();
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
				string master_symbol = Instrument.MasterInstrument.Name.ToLower();
				string resolved_filename = filename.Replace("<symbol>", master_symbol);
				if (!File.Exists(resolved_filename))
				{
					Log("File " + resolved_filename + " not found", LogLevel.Error);
					return;
				}

				var fileModifiedDate = File.GetLastWriteTime(resolved_filename);
				if(fileModifiedDate == _lastFileModifiedDate)
				{
					return;
				}
				_lastFileModifiedDate = fileModifiedDate;
				_levelsByDate = new LevelsDictionary();
				// reset cache as mapping may be invalid with new file
				resetCache();
				long maxLevels = 0;
				CultureInfo culture = new CultureInfo("en-US");
				using(StreamReader reader = new StreamReader(resolved_filename))
				{
					string line = reader.ReadLine();
					// skip header
					line = reader.ReadLine();
					int levels_index = 1;
					int symbol_index = -1;
					if(line.Split('|').Count() == 3)
					{
						symbol_index = 1;
						levels_index = 2;
						Log("Detected multi instrument version of input file", LogLevel.Information);
					}
					else 
					{
						Log("Detected single instrument version of input file", LogLevel.Information);
					}
				 
					while (line != null) 
					{
						string[] fields = line.Split('|');
						DateTime date = DateTime.ParseExact(fields[0].Trim(), "yyyy-MM-dd", culture);
						if(symbol_index == 1)
						{
							var symbol = fields[symbol_index].ToLower().Trim();
							if(symbol != master_symbol)
							{
								line = reader.ReadLine();
								continue;
							}
						}
						_levelsByDate.Add(date.Date, new Levels());
						string [] levels =  fields[levels_index].Split(',');
						maxLevels = Math.Max(levels.Length, maxLevels);
						foreach(string level in levels)
						{				
							_levelsByDate[date.Date][double.Parse(level, culture)] = false;
						}
						
						line = reader.ReadLine();
					}
				}
				Log(Name + " read file " + resolved_filename + " with modified date of " + _lastFileModifiedDate + " and maxLevels " + maxLevels, LogLevel.Information);
				
				foreach(var currentDateKV in _levelsByDate)
				{
					foreach(var oldDatesKV in _levelsByDate.Where(x => (x.Key < currentDateKV.Key && ((currentDateKV.Key - x.Key).Days <= MaxPriorDaysInDay))))
					{
						foreach(var levelsKV in oldDatesKV.Value)
						{
							if(!currentDateKV.Value.ContainsKey(levelsKV.Key))
							{
								currentDateKV.Value[levelsKV.Key] = true;
								if(VerboseLogging)
								{
									Log(String.Format("For date {0} will include price {3} from historical date {1} max prior days {2}",  currentDateKV.Key.ToString(), oldDatesKV.Key.ToString(), MaxPriorDaysInDay, levelsKV.Key), LogLevel.Information);
								}
							}
						}
					}
				}
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
					CarriedLevelLineColor = Brushes.Thistle;
					PriorDayLevelLineColor = Brushes.Orange;
					MaxDayCarryForward = 3;
					MaxPriorDaysInDay = 7;
					VerboseLogging = false;
					resetCache();
					Log("Daily Price Levels by Cargocult version " + version, LogLevel.Information);
				}
				else if (State == State.Configure)
				{
					currentUpperLevel 			= new Series<double>(this);
					currentLowerLevel			= new Series<double>(this);
					readCSV(LevelFileName);
					long maxLevels = 25;
					foreach(var entry in _levelsByDate)
					{
						maxLevels = Math.Max(maxLevels, entry.Value.Count);
					}
				
					for(int i=0; i < maxLevels; ++i) 
					{
						AddPlot(new Stroke(LevelLineColor, LevelLineType, LevelLineWidth, LevelLineOpacity), PlotStyle.HLine, "level_" + i);
					}
				}
			}

			private DateLevelsPair getLevelsForDate()
			{
				var currentDate = Time[0].Date;
				if(_chartDateToLevelsDateCache.ContainsKey(currentDate))
				{
					return _chartDateToLevelsDateCache[currentDate];
				}
				else 
				{
					var kv = _levelsByDate.LastOrDefault(x => ( x.Key <= currentDate && (currentDate - x.Key).Days <= MaxDayCarryForward) );
					_chartDateToLevelsDateCache.Add(currentDate, kv);
					var levelsFileDate = kv.Key;
					var logLevel = levelsFileDate == currentDate ? LogLevel.Information : levelsFileDate < currentDate ? LogLevel.Warning : LogLevel.Error;
					Log(String.Format("Using file level date: {0} for chart date: {1} maxDayCarryFoward: {2} symbol: {3}", 
						levelsFileDate, currentDate, MaxDayCarryForward, Instrument.MasterInstrument.Name.ToLower()), logLevel);
					return kv;
				}
			}
				
			private bool isDateInvalid(DateLevelsPair levelsKV)
			{
				return levelsKV.Equals(default(DateLevelsPair));
			}
			
			protected override void OnBarUpdate()
			{
				readCSV(LevelFileName);
				if (_levelsByDate == null) return;
				
				var currentDate = Time[0].Date;
				
				var levelsKV = getLevelsForDate();
				
				if(isDateInvalid(levelsKV))
				{
					if(VerboseLogging) 
					{
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
					if(VerboseLogging) 
					{
						var logLevel = levelsFileDate == currentDate ? LogLevel.Information : levelsFileDate < currentDate ? LogLevel.Warning : LogLevel.Error;
						Log(String.Format("Using file level date: {0} for chart date: {1} maxDayCarryFoward: {2} symbol: {3}", 
							levelsFileDate, currentDate, MaxDayCarryForward, Instrument.MasterInstrument.Name.ToLower()), logLevel);
					}

					foreach(var levelKV in levelsKV.Value)
					{
						if(counter >= Values.Length) 
						{
							Log("More levels added than plots - please refresh your chart using F5", LogLevel.Error);
							return;
						}
						
						PlotBrushes[counter][0] = levelKV.Value ? PriorDayLevelLineColor : levelsFileDate == currentDate ? LevelLineColor : CarriedLevelLineColor;
						
						var level = levelKV.Key;
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
			[Display(Name="Level Line Width", Description="Width of level line", Order=4, GroupName="Level Parameters")]
			public int LevelLineWidth
			{ get; set; }

			[Range(1, 100)]
			[Display(Name="Level Line Opacity", Description="Opacity of level line", Order=5, GroupName="Level Parameters")]
			public int LevelLineOpacity
			{ get; set; }

			[Display(Name="Level Line Type", Description="Type of line (line, dot, dash, etc.)", Order=6, GroupName="Level Parameters")]
			public DashStyleHelper LevelLineType
			{ get; set; }
			
			[Display(Name="Max Day Carry Forward", Description="Use the most recent date's data if current date less than or equal to this many days ahead in time", Order=9, GroupName="Level Parameters")]
			[Range(0, 10000)]
			public int MaxDayCarryForward
			{ get; set; }

			[XmlIgnore()]
			[Display(Name = "Carry Forward Level Line Color", Description = "Color of carry forward level line", Order = 10, GroupName="Level Parameters")]
			public Brush CarriedLevelLineColor
			{ get; set; }

			
			[Browsable(false)]
			public string CarriedLineColorSerialize
			{
				get { return Serialize.BrushToString(CarriedLevelLineColor); }
	   			set { CarriedLevelLineColor = Serialize.StringToBrush(value); }
			}
			
			[Display(Name="Max Prior Days In Day", Description="Use the most recent date's data if current date less than or equal to this many days ahead in time", Order=11, GroupName="Level Parameters")]
			[Range(0, 10000)]
			public int MaxPriorDaysInDay
			{ get; set; }

			[XmlIgnore()]
			[Display(Name = "Prior Day Line Color", Description = "Color of carry forward level line", Order = 12, GroupName="Level Parameters")]
			public Brush PriorDayLevelLineColor
			{ get; set; }

			
			[Browsable(false)]
			public string PriorDayLevelLineColorSerialize
			{
				get { return Serialize.BrushToString(PriorDayLevelLineColor); }
	   			set { PriorDayLevelLineColor = Serialize.StringToBrush(value); }
			}

			
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
		public CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName)
		{
			return CargocultDailyPriceLevels(Input, levelFileName);
		}

		public CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input, string levelFileName)
		{
			if (cacheCargocultDailyPriceLevels != null)
				for (int idx = 0; idx < cacheCargocultDailyPriceLevels.Length; idx++)
					if (cacheCargocultDailyPriceLevels[idx] != null && cacheCargocultDailyPriceLevels[idx].LevelFileName == levelFileName && cacheCargocultDailyPriceLevels[idx].EqualsInput(input))
						return cacheCargocultDailyPriceLevels[idx];
			return CacheIndicator<CargocultDailyPriceLevels>(new CargocultDailyPriceLevels(){ LevelFileName = levelFileName }, input, ref cacheCargocultDailyPriceLevels);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName)
		{
			return indicator.CargocultDailyPriceLevels(Input, levelFileName);
		}

		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input , string levelFileName)
		{
			return indicator.CargocultDailyPriceLevels(input, levelFileName);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(string levelFileName)
		{
			return indicator.CargocultDailyPriceLevels(Input, levelFileName);
		}

		public Indicators.CargocultDailyPriceLevels CargocultDailyPriceLevels(ISeries<double> input , string levelFileName)
		{
			return indicator.CargocultDailyPriceLevels(input, levelFileName);
		}
	}
}

#endregion
