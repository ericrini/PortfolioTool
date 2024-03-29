# Portfolio Rebalancer Tool
This a tool designed to align the holdings in a brokerage account to a model portfolio. Why pay a financial advisor hundreds of dollars to do something that literally takes 20 minutes?

# Model Files
The model files are JSON data files that specify the current holdings and the model to align to. You must create at least one model file for the tool to process. Each file includes the following data.

- Name is a friendly name for this portfolio. Since the tool can process multiple model files at once, this helps to
organize the output.

- Cash is any unallocated cash, or additional funding to add to the account. If you have a marginable account, you can enter the amount of margin you'd like to use here also.

- Allocations are the target percentages expressed as a decimal in the range of 0-1 inclusive. The sum of all allocations should be exactly 1.

- Holdings is the number of shares held in any current position in the portfolio. The current market value of these shares, plus the unallocated cash should add up to the total value of the portfolio.

Here is an example of a valid model file.

```
{
    "name": "INDIVIDUAL",
    "cash": 12571.19,
    "holdings": {
        "IEFA": 388,
        "IEMG": 96,
        "IJH": 40,
        "IJR": 28,
        "IVV": 85,
        "USRT": 68
    },
    "allocations": {
        "IVV": 0.30,
        "IJH": 0.05,
        "IJR": 0.03,
        "USRT": 0.03,
        "IEFA": 0.24,
        "IEMG": 0.05,
        "ISTB": 0.14,
        "IMTB": 0.08,
        "IAGG": 0.08
    }
}
```

# Running the tool.
You can run the tool by doing the following from the command line. The .\data parameter is the path to the folder containing your model files. The tool will attempt to process any .json file in this folder.

```
> dotnet run .\data
```

You should get output like this.

```
INDIVIDUAL ($68,229.46)
  IAGG
    Current holding is 0.00%, the target allocation is 8.00%.
    To align the holding, BUY 104 shares LIMIT at $52.20 each for a total of $5,428.80.
    To limit loss, STOP at $46.98.
  IEFA
    Current holding is 30.00%, the target allocation is 24.00%.
    To align the holding, SELL 25 shares LIMIT at $58.11 each for a total of ($1,452.75).
    To limit loss, STOP at $52.30.
  IEMG
    Current holding is 10.00%, the target allocation is 5.00%.
    To align the holding, SELL 7 shares LIMIT at $50.02 each for a total of ($350.14).
    To limit loss, STOP at $45.02.
```

 Entering these orders into your broker's trade sheet will align this portfolio to to the model allocations. Here's some helpful hints to get the best results.

- Build the portfolio from funds that have low expense ratios and no trading fees.
- Choose a day with low volatility to place orders.
- Wait until the market closes to generate the order recommendations.
- Use a limit order with a 1 day period of execution.
- Verify the total transaction amount from the application and the trade confirmation sheet match before confirming an order.
- In accounts with margin available (EX: individual brokerage account), you can place all the buy and sell orders at once. In other accounts (EX: 401k account) you will need to place all the SELL orders and wait for them to clear before making the BUY orders, unless there is sufficient liquid cash in the account to cover the BUY order before the SELL orders clear.
- Do not run this tool more frequently than once every 3-4 days or you will run into issues excessively trading unsettled funds.
- This tool suggests a stop limit order but a trailing stop may make more sense for longer term "fire and forget" type investments.
