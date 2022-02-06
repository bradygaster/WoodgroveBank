# Woodgrove Bank

This repo shows off a banking app built using .NET, Kubernetes, and Azure. 

## Business Processes Represented

- [ ] Customers can check their `Account` `Balance`.
- [ ] Customers can withdraw money from an `Account` with an `Balance` greater than the sum of the withdrawal's `WithdrawalAmount` minus 2.00, thus, `withdrawalSucceeds = ((balance - (withdrawalamount+2)) > balance)`.
- [ ] Customers can deposit money into an `Account`. Each deposit should be recorded with its own `TransactionId` and `DepositAmount`, as well as the `AccountId` into which it was deposited, and the `Balance` and `ResultingBalance` of the transaction.
- [ ] When a customer deposits more than 10,000, the `HugeDeposit` alert is recorded. The `HugeDeposit` alert requires the `CustomerId`, `TransactionId`, `AccountId`, and `Amount` properties to be recorded correctly.
- [ ] Customers can withdraw money from an `Account`. Each withdrawal should be recorded with its own `TransactionId` and `WithdrawalAmount`, as well as the `AccountId` from which it was withdrawn, and the `Balance` and `ResultingBalance` of the transaction.
- [ ] When a customer attempts to withdraw more than is in their `Account`, the `AttemptedHack` alert is recorded. The `AttemptedHack` alert requires the `CustomerId`, `AccountId`, `WithdrawalAmount`, `TransactionId`, and `Balance` properties to be recorded correctly.
- [ ] Any `Account` triggering either the `HugeDeposit` or `AttemptedHack` alerts is flagged with the `IsUnderObservation` flag. 
- [ ] Every transaction must be recorded, no matter the result of the `Balance` or success factor. Each transaction has a `Deposit` or `Withdrawal` identifier, `Success` property, an `AccountId` property, and a `TransactionId` property. 
- [ ] Bank employees have a view of all `Account`s and a detail view of each account showing each `Withdrawl` and each `Deposit`.

### Technical stretch goals

- Failover to second cloud provider for back-end APIs.
- Failover to second cloud provider for front-end experiences.

### Stretch goals

- [ ] Each time a `HugeDeposit` alert is recorded, a Teams notification is sent to the **Laundering Investigation** Teams channel.
- [ ] Any account flagged with the `IsUnderObservation` flag should be frozen from subsequent withdrawals.
- [ ] When the `AttemptedHack` alert is recorded, the account in question is flagged with the `IsUnderObservation` flag.

## Technical components

- [ ] Front End
  - [ ] ATM front end for customers
  - [ ] Alerts front end for employees
  - [ ] Transactions frond end for employees
- [ ] Back End
  - [ ] ATM withdrawal API
  - [ ] ATM deposit API
  - [ ] API to flag account as `IsUnderObservation`
  - [ ] Account summary API
  - [ ] Alert collection API

