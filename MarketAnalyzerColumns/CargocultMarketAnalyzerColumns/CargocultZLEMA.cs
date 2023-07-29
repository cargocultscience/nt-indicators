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
	public class CargocultZLEMA : MarketAnalyzerColumn
	{
		private NinjaTrader.NinjaScript.Indicators.CargocultZLEMA indicator;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Market Analyzer Column For Keltner Channel";
				Name										= "Zelma";
				Calculate									= Calculate.OnBarClose;
				DataType   									= typeof(double);
        		IsEditable 									= false;
				FormatDecimals 								= 2;
				
			}
			else if (State == State.DataLoaded)
			{
				indicator		= CargocultZLEMA(21);
				
			}
		}

		public enum ValueTypeEnum
		{
			Value,
			TicksFromValue,
			Slope
		}

		
		protected override void OnMarketData(MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.NaN;

			if (marketDataUpdate.MarketDataType == MarketDataType.Last)
			{
			  	switch(ValueType) 
			  	{				
					case ValueTypeEnum.Value:
						CurrentValue = indicator.Value[0]; break;
					case ValueTypeEnum.TicksFromValue:
						CurrentValue = Math.Round((marketDataUpdate.Price - indicator.Value[0]) / Instrument.MasterInstrument.TickSize); break;
					case ValueTypeEnum.Slope:
						CurrentValue = LinRegSlope(indicator.Value, Periods)[0]; break;
				}
			}
		}

		
		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Value Type", Description="Which value should be used for the column", Order=1, GroupName="Params")]
		public ValueTypeEnum ValueType
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Periods", Description="Periods For Linear Regression Slipe", Order=2, GroupName="Params")]
		public int Periods
		{ get; set; }
		
		#endregion
	}
}
