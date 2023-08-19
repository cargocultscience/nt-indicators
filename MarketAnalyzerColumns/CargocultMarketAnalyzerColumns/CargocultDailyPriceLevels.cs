#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;

#endregion

//This namespace holds MarketAnalyzerColumns in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public class CargocultDailyPriceLevels : MarketAnalyzerColumn
	{
		private NinjaTrader.NinjaScript.Indicators.CargocultDailyPriceLevels indicator;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Market Analyzer Column For Daily Price Levels";
				Name										= "Daily Price Levels";
				Calculate									= Calculate.OnPriceChange;
				DataType   									= typeof(double);
        		IsEditable 									= false;
				FormatDecimals 								= 2;
				LevelsFilename                              = "C:\\dir\\<symbol>_levels.csv";
			}
			else if (State == State.DataLoaded)
			{
				// ::Filename format - Indicator will replace the text <symbol> with whatever the base symbol name is (eg spy, es, nq, meq, mes)
				string resolved_filename = LevelsFilename.Replace("<symbol>", Instrument.MasterInstrument.Name.ToLower());
				indicator = CargocultDailyPriceLevels(resolved_filename);
			}
		}

		public enum ValueTypeEnum
		{
			UpperLevel,
			LowerLevel,
			TicksFromUpper,
			TicksFromLower
		}

		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.NaN;
			
			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
		  	switch(ValueType) 
		  	{				
				case ValueTypeEnum.UpperLevel:
					CurrentValue = indicator.CurrentUpperLevel[0]; break;
				case ValueTypeEnum.LowerLevel:
					CurrentValue = indicator.CurrentLowerLevel[0]; break;
				case ValueTypeEnum.TicksFromUpper:
					//Log("upper level " + indicator.CurrentUpperLevel[0] + " last: " + marketDataUpdate.Last, LogLevel.Information);
					CurrentValue = (indicator.CurrentUpperLevel[0] - marketDataUpdate.Price) / Instrument.MasterInstrument.TickSize; break;
				case ValueTypeEnum.TicksFromLower:
					CurrentValue = (marketDataUpdate.Price - indicator.CurrentLowerLevel[0]) / Instrument.MasterInstrument.TickSize; break;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Levels Filename", Description="Name of the csv file containing the price levels", Order=1, GroupName="Level Parameters")]
		public string LevelsFilename
		{ get; set; }
				
		[NinjaScriptProperty]
		[Display(Name="Display Value", Description="Type of value to display", Order=2, GroupName="Level Parameters")]
		public ValueTypeEnum ValueType
		{ get; set; }
		
		#endregion
	}
}
