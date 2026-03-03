# PetiteMaisonEpouvante.Solution

This repository contains a simple .NET 8 solution with an API, a Blazor UI, and tests.

## Running with Docker Compose

```powershell
docker compose up -d --build
```

Access UI at https://localhost and API at https://api.localhost.

## Running on Kubernetes (Docker Desktop)

Docker Desktop includes a single-node Kubernetes cluster. After enabling it, you can deploy the application using the provided manifests.

1. Build container images locally (they will be available to the local cluster):
   ```powershell
   docker build -t petitemaisonepouvante.api:latest PetiteMaisonEpouvante.API
   docker build -t petitemaisonepouvante.ui:latest PetiteMaisonEpouvante.UI
   ```
   (alternatively use `skaffold` or similar tools)

2. Apply manifests:
   ```powershell
   kubectl apply -f k8s/namespace.yaml
   kubectl apply -f k8s/jwt-secret.yaml
   kubectl apply -f k8s/db-deployment.yaml
   kubectl apply -f k8s/api-deployment.yaml
   kubectl apply -f k8s/ui-deployment.yaml
   kubectl apply -f k8s/ingress.yaml
   ```

3. Verify pods and services:
   ```powershell
   kubectl get pods -n petitemaisonepouvante
   kubectl get svc -n petitemaisonepouvante
   ```

4. Edit your `hosts` file if needed to map `localhost` to your cluster IP.

The UI will be available at `http://localhost` and the API at `http://localhost/api` once the ingress is working.

## Configuration

- **API** reads connection string and JWT key from environment variables (`ConnectionStrings__DefaultConnection`, `Jwt__Key`), making it easy to override in Kubernetes via `env` in the deployment or ConfigMap/Secret.
- **UI** now uses `builder.HostEnvironment.BaseAddress` so it works both in Docker Compose and when served back-to-back with the API through an ingress controller.

## CI/CD

The existing GitHub Actions workflow builds the solution and runs all tests. You can optionally extend it to push images to a registry and deploy the `k8s/` manifests to a cluster of your choice.

---

Feel free to adapt the manifest templates (e.g. add PVCs, resource requests, ingress TLS, etc.) for a production scenario.