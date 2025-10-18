#!/bin/bash

# Monolith vs Microservices Architecture Runner
# Simple bash script that runs Docker Compose commands

set -e

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

# Functions
print_success() { echo -e "${GREEN}✅ $1${NC}"; }
print_error() { echo -e "${RED}❌ $1${NC}"; }
print_info() { echo -e "${YELLOW}ℹ️  $1${NC}"; }

# Main execution
case "${1:-help}" in
    "monitoring")
        print_info "Starting monitoring stack..."
        docker-compose -f docker-compose.monitoring.yml up -d
        print_success "Monitoring started! Grafana: http://localhost:3000, Prometheus: http://localhost:9090"
        ;;
    "monolith")
        print_info "Starting monolith..."
        docker-compose -f docker-compose.monolith.yml up --build -d
        print_success "Monolith started! App: http://localhost:6000"
        ;;
    "microservices")
        print_info "Starting microservices..."
        docker-compose -f docker-compose.microservices.yml up --build -d
        print_success "Microservices started! API Gateway: http://localhost:7000"
        ;;
    "monolith-full")
        print_info "Starting monitoring + monolith..."
        docker-compose -f docker-compose.monitoring.yml up -d
        sleep 3
        docker-compose -f docker-compose.monolith.yml up --build -d
        print_success "Full monolith stack running! Grafana: http://localhost:3000, App: http://localhost:6000"
        ;;
    "microservices-full")
        print_info "Starting monitoring + microservices..."
        docker-compose -f docker-compose.monitoring.yml up -d
        sleep 3
        docker-compose -f docker-compose.microservices.yml up --build -d
        print_success "Full microservices stack running! Grafana: http://localhost:3000, API Gateway: http://localhost:7000"
        ;;
    "all")
        print_info "Starting everything (monitoring + monolith + microservices)..."
        docker-compose -f docker-compose.monitoring.yml up -d
        sleep 3
        docker-compose -f docker-compose.monolith.yml up --build -d
        sleep 3
        docker-compose -f docker-compose.microservices.yml up --build -d
        print_success "Everything started! Grafana: http://localhost:3000, Monolith: http://localhost:6000, API Gateway: http://localhost:7000"
        ;;
    "stop-all")
        print_info "Stopping all services..."
        docker-compose -f docker-compose.monitoring.yml down
        docker-compose -f docker-compose.monolith.yml down
        docker-compose -f docker-compose.microservices.yml down
        print_success "All services stopped!"
        ;;
    "status")
        print_info "Current status:"
        docker-compose -f docker-compose.monitoring.yml ps
        docker-compose -f docker-compose.monolith.yml ps
        docker-compose -f docker-compose.microservices.yml ps
        ;;
    "help"|"-h"|"--help"|*)
        echo -e "${BLUE}Usage: ./run.sh [command]${NC}"
        echo ""
        echo "Commands:"
        echo "  monitoring        Start monitoring stack only"
        echo "  monolith         Start monolith application only"
        echo "  microservices    Start microservices stack only"
        echo "  monolith-full    Start monitoring + monolith"
        echo "  microservices-full Start monitoring + microservices"
        echo "  all              Start everything (monitoring + monolith + microservices)"
        echo "  stop-all         Stop all services"
        echo "  status           Show current status"
        echo "  help             Show this help"
        echo ""
        echo "Examples:"
        echo "  ./run.sh all"
        echo "  ./run.sh monolith-full"
        echo "  ./run.sh microservices-full"
        echo "  ./run.sh stop-all"
        ;;
esac