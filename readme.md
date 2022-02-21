# Woodgrove Bank

This repo shows off a banking app built using .NET, Kubernetes, and Azure. 

## Setup

There are a variety of ways the Woodgrove Bank apps can be set up on Kubernetes or on Azure PaaS.

## Kubernetes

Presuming a local Kubernetes setup. In the case of this document, Docker Desktop was used, along with the Kubernetes that ships with it. A "Reset Kubernetes Cluster" was performed to reset the cluster.

1. Working Namespace

    Create a new namespace in your Kubernetes cluster for your deployment by executing this command. This way if you make a mistake you can easily reset by deleting the namespace and starting over.

    ```
    kubectl create namespace woodgrovebank01
    kubectl config set-context --current --namespace=woodgrovebank01 
    ```

2. If you haven't yet already installed ingress-nginx, do so next:

    ```bash
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.1/deploy/static/provider/baremetal/deploy.yaml
    ```

3. Install KEDA to enable silo auto-scaling with the experimental Orleans Scaler:

    ```bash
    helm repo add kedacore https://kedacore.github.io/charts
    helm repo update
    helm install keda kedacore/keda --namespace woodgrovebank01
    ```

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
  kubectl apply -f ./k8s/admin.yaml
  ```

- Deploy the API

  ```
  kubectl apply -f ./k8s/api.yaml
  ```

- Deploy the dashboard

  ```
  kubectl apply -f ./k8s/dashboard.yaml
  ```

- Set up the port-forwarding so you can browse all 3 sites. Execute each of the following commands in a separate terminal window.

  ```
  kubectl port-forward service/woodgrovebank-admin 5001:80
  kubectl port-forward service/woodgrovebank-api 5000:80
  kubectl port-forward service/woodgrovebank-dashboard 8080:80
  ```
- Open your browser to the [Woodgrove Bank Employee App](http://localhost:5001/customers).
- Open another browser tab to the [Swagger UI page for the Woodgrove Bank API](http://localhost:5000/swagger)
- Open a third browser tab to the [The Orleans Dashboard](http://localhost:8080). 

- Run the data-generating app to generate fake data. 

  ```
  cd BogusGenerator
  dotnet run
  ```
  
  This console app will generate some fake data. As you use the Swagger UI page and the Bogus generator to create fake data, you'll see more grains appear in the cluster and in the front end interface. Add one round of customers and accounts. 
  
### Auto-scaling the Orleans Cluster by Grain Count

Now you'll add the Orleans Scaler (experimental) to the Kubernetes cluster see how you can scale your silos according to the number of a specific type of grain you expect to see in the Orleans cluster, per silo.

- Deploy the scaler

  ```
  kubectl apply -f ./k8s/scaler.yaml
  ```

- Watch the logs for the scaler. Here, I pass them to `jq` so they come out nicely-formatted on-screen.

  ```
  kubectl logs -f <orleans-scaler-pod-name> | jq
  ```

  Now you should see JSON-formatted logs on-screen. You may need to [install jq](https://stedolan.github.io/jq/), first, but if you don't want to, just omit the `| jq` part and you should be good.


