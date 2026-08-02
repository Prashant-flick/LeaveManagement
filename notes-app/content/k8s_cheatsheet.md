# Azure & Kubernetes (kubectl) Developer Cheatsheet

This cheatsheet provides the most important commands to manage, inspect, and debug your microservices cluster in Azure.

---

## 1. Kubernetes Inspection Commands (Checking Status)

Use these commands to see the health and status of your applications.

### View running containers (Pods)
```bash
# List all pods and their status (Running, Pending, Error, etc.)
kubectl get pods

# List pods with more details (like IP address and Node name)
kubectl get pods -o wide

# Watch pod status changes in real-time (press Ctrl+C to exit)
kubectl get pods -w
```

### View Services & Public IPs (API Gateway)
```bash
# View the public IP of your Ingress Controller (API Gateway)
kubectl get svc -n ingress-nginx

# View all services in the default namespace
kubectl get svc
```

### View Ingress Routing Rules
```bash
# List Ingress path mappings (e.g. /api/auth -> auth-service)
kubectl get ingress
```

### View Everything at Once
```bash
# View all Pods, Services, Deployments, and ReplicaSets in one view
kubectl get all
```

---

## 2. Debugging & Logs (Finding Errors)

If a service is crashing or not working, use these commands to inspect the logs.

### Read Application Logs
```bash
# View logs for a deployment (replaces auth-service with employee-service, leave-service, or postgres-db)
kubectl logs deployment/auth-service

# Watch logs stream in real-time (-f means follow)
kubectl logs deployment/auth-service -f

# View logs for a specific pod name (e.g., auth-service-5fb9989f9-l6qzp)
kubectl logs <POD_NAME>
```

### Diagnose Crashed Pods
If a pod has status `CrashLoopBackOff` or `ErrImagePull`, run:
```bash
kubectl describe pod <POD_NAME>
```
*Look at the "Events" section at the bottom to see why it failed to download or launch.*

### Connect Inside a Container (SSH-like)
```bash
# Open a shell inside an active container
kubectl exec -it <POD_NAME> -- /bin/sh

# Connect directly to the PostgreSQL Database CLI inside the cluster
kubectl exec -it deployment/postgres-db -- psql -U postgres -d employeeleave
```

---

## 3. Rollout & Update Commands

Use these commands when you want to deploy code updates.

### Restart a service (Force pull fresh images)
Since we are using the `latest` image tag, Kubernetes needs to be forced to restart and fetch the updated code:
```bash
kubectl rollout restart deployment/auth-service
kubectl rollout restart deployment/employee-service
kubectl rollout restart deployment/leave-service
```

### Check Rollout Status
```bash
kubectl rollout status deployment/auth-service
```

---

## 4. Azure CLI Commands (Cloud Resource Management)

Use these commands to control your subscription and billing.

### Re-Link Your Terminal (If you change machines or sessions)
If `kubectl` stops responding, download the AKS access keys again:
```bash
az aks get-credentials --resource-group LeaveManagementIndiaRG --name leave-aks --overwrite-existing
```

### List Active Resource Groups
```bash
az group list -o table
```

### Delete Everything to STOP Costs
```bash
az group delete --name LeaveManagementIndiaRG --yes --no-wait
```
