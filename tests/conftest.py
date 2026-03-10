import subprocess
import time
import pytest
import urllib3
from playwright.sync_api import Page, BrowserContext, Browser

# Suppress SSL warnings for localhost
urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

BASE_URL = "https://localhost:5001"


@pytest.fixture(scope="session")
def browser_context_args():
    return {"ignore_https_errors": True}


@pytest.fixture(scope="session", autouse=True)
def _start_blazor_server():
    """Start the Blazor Server app before tests, stop after."""
    proc = subprocess.Popen(
        [
            "dotnet",
            "run",
            "--project",
            "../XafSecureDash/XafSecureDash.Blazor.Server",
        ],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )

    # Wait for server to be ready
    import requests

    for _ in range(60):
        try:
            r = requests.get(BASE_URL, verify=False, timeout=2)
            if r.status_code < 500:
                break
        except Exception:
            pass
        time.sleep(2)
    else:
        proc.kill()
        raise RuntimeError("Blazor Server did not start within 120 seconds")

    yield

    proc.kill()
    proc.wait()


def xaf_login(page: Page, username: str, password: str = ""):
    """Log in to XAF Blazor via the login page."""
    page.goto(BASE_URL)
    # XAF Blazor login page has input fields for UserName and Password
    page.wait_for_selector('input[aria-label="User Name"]', timeout=30_000)
    page.fill('input[aria-label="User Name"]', username)
    if password:
        page.fill('input[aria-label="Password"]', password)
    page.click('div.xaf-logon >> button:has-text("Log In")')
    # Wait for the main page to load (navigation sidebar appears)
    page.wait_for_selector(".xaf-navigation", timeout=30_000)


def xaf_logout(page: Page):
    """Log out of XAF Blazor."""
    # Click user menu / Log Off
    page.click('.xaf-action[data-action-id="LogOff"]')
    page.wait_for_selector('input[aria-label="User Name"]', timeout=15_000)


def navigate_to(page: Page, *menu_path: str):
    """Navigate through XAF menu items. E.g. navigate_to(page, 'Reports', 'Secure Dashboard Data')"""
    for item in menu_path:
        page.click(f'.xaf-navigation >> text="{item}"')
        page.wait_for_timeout(500)


def get_dashboard_titles(page: Page) -> list[str]:
    """Navigate to the dashboard list and return all visible dashboard titles."""
    navigate_to(page, "Reports")
    # Click the dashboard nav item
    page.click('.xaf-navigation >> text="Secure Dashboard Data"')
    page.wait_for_selector(".dxgvHSDC, .xaf-listview", timeout=15_000)
    page.wait_for_timeout(1000)
    # Get all visible rows — XAF grid cells with Title column
    titles = page.locator('.xaf-listview [class*="GridRow"] td:first-child').all_text_contents()
    # Clean whitespace
    return [t.strip() for t in titles if t.strip()]
