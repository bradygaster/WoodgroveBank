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

1. If you haven't yet already installed ingress-nginx, do so next:

    ```bash
    kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/controller-v1.1.1/deploy/static/provider/baremetal/deploy.yaml
    ```

1. Per the [Orleans Kubernetes](https://learn.microsoft.com/dotnet/orleans/deployment/kubernetes) docs, for RBAC-enabled clusters, the Kubernetes service account for the pods may also need to be granted the required access. `rolebinding.yaml` performs this routine on the Kubernetes cluster. 

    ```bash
    kubectl apply -f ./k8s/rolebinding.yaml
    kubectl apply -f ./k8s/dnsutils.yaml
    ```

1. Install KEDA to enable silo auto-scaling with the experimental Orleans Scaler:

    ```bash
    helm repo add kedacore https://kedacore.github.io/charts
    helm repo update
    helm install keda kedacore/keda --namespace woodgrovebank01
    ```

1. Create a new Azure Storage account (or just use Azurite)

      You can either use Azurite to emulate the Azure Storage Service running right in your Kubernetes instance (not recommended for production usage, this is *only* for dev-time deployments). If you'd like to use Azurite, execute this command:

      > Note: Do **NOT** execute both of the below commands. Only use one or the other. 

      ```bash
      kubectl apply -f ./k8s/azurite.yaml
      kubectl create secret generic storage-connection-strings --from-literal=clustering="UseDevelopmentStorage=true;DevelopmentStorageProxyUri=http://azurite"
      ```
        
      **Or**, if you've created your own Azure Storage Account in the cloud, execute this command: 

        kubectl create secret generic storage-connection-strings --from-literal=clustering="<connection-string-from-portal>"

1. Build the containers using `docker-compose`. 

    ```bash
    docker-compose build
    ```

1. Now install all the services.

    ```bash
    kubectl apply -f ./k8s/api.yaml
    kubectl apply -f ./k8s/admin.yaml
    kubectl apply -f ./k8s/simulations.yaml
    kubectl apply -f ./k8s/dashboard.yaml
    ```

1. Browse to the various microservices you just deployed. 

- Open your browser to the [Woodgrove Bank Employee App](http://localhost:31001/customers).
- Open another browser tab to the [Swagger UI page for the Woodgrove Bank API](http://localhost:31000/swagger).
- Open a third browser tab to the [The Orleans Dashboard](http://localhost:31002). 

1. (Optional) Run the data-generating app to generate fake data. 

  ```bash
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


