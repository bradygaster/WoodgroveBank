# Woodgrove Bank

This repo shows off a banking app built using .NET, Kubernetes, and Azure. 

## Setup

There are a variety of ways the Woodgrove Bank apps can be set up on Kubernetes or on Azure PaaS.

## Kubernetes

Presuming a local Kubernetes setup. In the case of this document, Docker Desktop was used, along with the Kubernetes that ships with it. A "Reset Kubernetes Cluster" was performed to reset the cluster.

Set up the Kubernetes cluster with the prerequisite Kubernetes services:

- Keda

  ```bash
  helm repo add kedacore https://kedacore.github.io/charts
  helm repo update
  kubectl create namespace keda
  helm install keda kedacore/keda --namespace keda
  ```
- Ingress

  ```bash
  kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.1/deploy/static/provider/baremetal/deploy.yaml
  ```
- Working Namespace

  Create a new namespace in your Kubernetes cluster for your deployment by executing the command:

  ```
  kubectl create namespace woodgrovebank01
  kubectl config set-context --current --namespace=woodgrovebank01 
  ```

  This way if you make a mistake you can easily reset by deleting the namespace and starting over.

- Create a new Azure Storage account

  Once you've created the storage account, get the connection string for the storage account. Then use it in this command, which sets the Storage Account's connection string as a secret in the Kubernetes cluster. 

  ```
  kubectl create secret generic storage-connection-strings --from-literal=clustering="<connection-string>"
  ```

- Build the containers

  Use `docker-compose` to build the containers. 

  ```
  docker-compose build
  ```

- Deploy the bank employee app

  ```
  kubectl apply -f .\k8s\admin.yaml
  ```

- Deploy the API

  ```
  kubectl apply -f .\k8s\api.yaml
  ```

- Deploy the dashboard

  ```
  kubectl apply -f .\k8s\dashboard.yaml
  ```

- Set up the port-forwarding so you can browse all 3 sites. Execute each of the following commands in a separate terminal window.

  ```
  kubectl port-forward service/woodgrovebank-admin 5001:80
  kubectl port-forward service/woodgrovebank-api 5000:80
  kubectl port-forward service/woodgrovebank-dashboard 8080:80
  ```

- Open your browser to the [The Orleans Dashboard](http://localhost:8080). 
- Open another browser tab to the [Woodgrove Bank Employee App](http://localhost:5001/customers).
- Open a third browser tab to the [Swagger UI page for the Woodgrove Bank API](http://localhost:5000/swagger)

- Run the data-generating app to generate fake data. 

  ```
  cd .\BogusGenerator\
  dotnet run
  ```

This console app will generate some fake data. As you use the Swagger UI page and the Bogus generator to create fake data, you'll see more grains appear in the cluster and in the front end interface.

---

## Business Processes Represented

- [ ] Customers can check their `Account` `Balance`.
- [x] Customers can withdraw money from an `Account` with an `Balance` greater than the sum of the withdrawal's `WithdrawalAmount` minus 2.00, thus, `withdrawalSucceeds = ((balance - (withdrawalamount+2)) > balance)`.
- [x] Customers can deposit money into an `Account`. Each deposit should be recorded with its own `TransactionId` and `DepositAmount`, as well as the `AccountId` into which it was deposited, and the `Balance` and `ResultingBalance` of the transaction.
- [ ] When a customer deposits more than 10,000, the `HugeDeposit` alert is recorded. The `HugeDeposit` alert requires the `CustomerId`, `TransactionId`, `AccountId`, and `Amount` properties to be recorded correctly.
- [x] Customers can withdraw money from an `Account`. Each withdrawal should be recorded with its own `TransactionId` and `WithdrawalAmount`, as well as the `AccountId` from which it was withdrawn, and the `Balance` and `ResultingBalance` of the transaction.
- [ ] When a customer attempts to withdraw more than is in their `Account`, the `AttemptedHack` alert is recorded. The `AttemptedHack` alert requires the `CustomerId`, `AccountId`, `WithdrawalAmount`, `TransactionId`, and `Balance` properties to be recorded correctly.
- [ ] Any `Account` triggering either the `HugeDeposit` or `AttemptedHack` alerts is flagged with the `IsUnderObservation` flag. 
- [x] Every transaction must be recorded, no matter the result of the `Balance` or success factor. Each transaction has a `Deposit` or `Withdrawal` identifier, `Success` property, an `AccountId` property, and a `TransactionId` property. 
- [ ] Bank employees have a view of all `Customer`s and a detail view of each account showing each `Account` and each `Transaction`. 
- [ ] Any account flagged with the `IsUnderObservation` flag should be frozen from subsequent withdrawals.
- [ ] When the `AttemptedHack` alert is recorded, the account in question is flagged with the `IsUnderObservation` flag.

## Technical components

- [ ] Front End
  - [x] ATM front end for customers
  - [ ] Alerts front end for employees
  - [ ] Transactions frond end for employees
- [ ] Back End
  - [x] ATM withdrawal API
  - [x] ATM deposit API
  - [ ] API to flag account as `IsUnderObservation`
  - [ ] Account summary API
  - [ ] Alert collection API

