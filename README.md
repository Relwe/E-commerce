# Selenium Grid Automation Project
## Overview

This project contains automated UI tests built with Selenium WebDriver, designed to run either locally or on a Selenium Grid powered by Docker.
It provides an easy, scalable way to execute tests in parallel across multiple browsers and environments — ideal for CI/CD pipelines.

## Key Features

Run tests on Selenium Grid (Dockerized hub + browser nodes)

docker compose -f docker/selenium-grid.yml down -v

Execute tests in parallel across Chrome and Firefox

Easily toggle headless mode for CI environments

Compatible with local runs, Docker containers, and GitHub Actions

Structured for clean separation of pages, tests, and infrastructure

Includes .runsettings file for environment configuration

## Project Structure

.
├── docker/
│   └── selenium-grid.yml         # Docker Compose for hub & nodes
├── tests/
│   ├── Infrastructure/           # DriverFactory, BaseTest, setup hooks
│   ├── Pages/                    # Page Object Model classes
│   ├── Tests/                    # NUnit test classes
│   └── tests.runsettings         # Test environment configuration
|   └── Utils/                    # All Utils files
├── .github/workflows/            # Optional CI/CD workflows
│   └── selenium-grid-ci.yml
└── README.md


## Running Selenium Grid with Docker

Start Selenium Grid:
docker compose -f docker/selenium-grid.yml up -d

Open the Selenium Grid UI:
http://localhost:4444/ui

Stop and clean up:
