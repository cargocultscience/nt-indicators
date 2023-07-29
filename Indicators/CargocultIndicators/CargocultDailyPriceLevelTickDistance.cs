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
		
		public class CargocultDailyPriceLevelTickDistance : Indicator
		{
			#region Variables
			private Dictionary<DateTime, List<double>> _levelsByDate;
			private DateTime _lastFileModifiedDate;
			private long _lastMaxLevels;
			private static string version = "1.0.0";
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
					Description									= @"Draws a line that represents the number of ticks away last price is from the nearest price level";
					Name										= "Cargocult Daily Price Levels Tick Distance";
					Calculate									= Calculate.OnEachTick;
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

					Log("Daily Price Levels Tick Distance by Cargocult version " + version, LogLevel.Information);
					//AddPlot(new Stroke(Brushes.SeaGreen, DashStyleHelper.Solid, 1, 100), PlotStyle.Line, "Tick Distance From Upper");
					//AddPlot(new Stroke(Brushes.Tomato, DashStyleHelper.Solid, 1, 100), PlotStyle.Line, "Tick Distance From Lower");
					AddPlot(new Stroke(Brushes.Wheat, DashStyleHelper.Dot, 1, 100), PlotStyle.Line, "Tick Distance From Closest");
					AddLine(Brushes.DarkGray, 0,			Custom.Resource.NinjaScriptIndicatorZeroLine);
				}
				else if (State == State.Configure)
				{
					readCSV(LevelFileName);
				}
			}

			protected override void OnBarUpdate()
			{
				readCSV(LevelFileName);
				if (_levelsByDate == null) return;
				if(_levelsByDate.ContainsKey(Time[0].Date)) 
				{
					double input0	= Input[0];
					var levels = _levelsByDate[Time[0].Date];
					double upper_level = double.NaN;
					double lower_level = double.NaN;
					double closest_level = double.NaN;
					foreach(double level in levels) 
					{
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
					//Log("level calc:: date: " + Time[0] + " lower level: " + lower_level + " upper level: " + upper_level + "closest level: " + closest_level + " price: " + input0, LogLevel.Information);
					//Values[0][0] = (input0 - upper_level) / Instrument.MasterInstrument.TickSize;
					//Values[1][0] = (input0 - lower_level) / Instrument.MasterInstrument.TickSize;
					TicksFromClosestLevel[0] = (input0 - closest_level) / Instrument.MasterInstrument.TickSize;
				}
			}
			
			#region Properties
			[Browsable(false)]
			[XmlIgnore]
			public Series<double> TicksFromClosestLevel
			{
	  			get { return Values[0]; }
			}
			
			[NinjaScriptProperty]
			[Display(Name="Level FileName", Description="Name of the csv file containing the price levels", Order=1, GroupName="Level Parameters")]
			public string LevelFileName
			{ get; set; }

			#endregion

		}
	}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CargocultDailyPriceLevelTickDistance[] cacheCargocultDailyPriceLevelTickDistance;
		public CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(string levelFileName)
		{
			return CargocultDailyPriceLevelTickDistance(Input, levelFileName);
		}

		public CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(ISeries<double> input, string levelFileName)
		{
			if (cacheCargocultDailyPriceLevelTickDistance != null)
				for (int idx = 0; idx < cacheCargocultDailyPriceLevelTickDistance.Length; idx++)
					if (cacheCargocultDailyPriceLevelTickDistance[idx] != null && cacheCargocultDailyPriceLevelTickDistance[idx].LevelFileName == levelFileName && cacheCargocultDailyPriceLevelTickDistance[idx].EqualsInput(input))
						return cacheCargocultDailyPriceLevelTickDistance[idx];
			return CacheIndicator<CargocultDailyPriceLevelTickDistance>(new CargocultDailyPriceLevelTickDistance(){ LevelFileName = levelFileName }, input, ref cacheCargocultDailyPriceLevelTickDistance);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(string levelFileName)
		{
			return indicator.CargocultDailyPriceLevelTickDistance(Input, levelFileName);
		}

		public Indicators.CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(ISeries<double> input , string levelFileName)
		{
			return indicator.CargocultDailyPriceLevelTickDistance(input, levelFileName);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(string levelFileName)
		{
			return indicator.CargocultDailyPriceLevelTickDistance(Input, levelFileName);
		}

		public Indicators.CargocultDailyPriceLevelTickDistance CargocultDailyPriceLevelTickDistance(ISeries<double> input , string levelFileName)
		{
			return indicator.CargocultDailyPriceLevelTickDistance(input, levelFileName);
		}
	}
}

#endregion
