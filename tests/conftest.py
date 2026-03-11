import subprocess
import time
import logging
import pytest
import urllib3
from playwright.sync_api import Page

urllib3.disable_warnings(urllib3.exceptions.InsecureRequestWarning)

BASE_URL = "https://localhost:5001"

logging.basicConfig(
    level=logging.INFO,
    format="%(asctime)s [%(levelname)s] %(message)s",
    datefmt="%H:%M:%S",
)
log = logging.getLogger("xaf-e2e")


@pytest.fixture(scope="session")
def browser_context_args():
    return {"ignore_https_errors": True}


@pytest.fixture(scope="session", autouse=True)
def _start_blazor_server():
    """Start the Blazor Server app before tests, stop after."""
    import requests

    try:
        r = requests.get(BASE_URL, verify=False, timeout=2)
        if r.status_code < 500:
            log.info("Server already running at %s", BASE_URL)
            yield
            return
    except Exception:
        pass

    log.info("Starting Blazor Server...")
    proc = subprocess.Popen(
        ["dotnet", "run", "--project", "../XafSecureDash/XafSecureDash.Blazor.Server"],
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    for attempt in range(60):
        try:
            r = requests.get(BASE_URL, verify=False, timeout=2)
            if r.status_code < 500:
                log.info("Server ready after %ds", attempt * 2)
                break
        except Exception:
            pass
        time.sleep(2)
    else:
        proc.kill()
        raise RuntimeError("Server did not start within 120s")

    yield
    proc.kill()
    proc.wait()


def xaf_login(page: Page, username: str, password: str = ""):
    """Log in to XAF Blazor."""
    log.info("Logging in as '%s'", username)
    page.goto(BASE_URL)
    page.wait_for_timeout(3000)
    username_input = page.locator('input.dxbl-text-edit-input[type="text"]').first
    username_input.wait_for(timeout=30_000)
    username_input.fill(username)
    if password:
        page.locator('input.dxbl-text-edit-input[type="password"]').first.fill(password)
    page.click('button[data-action-name="Log In"]')
    page.locator(".xaf-sidebar").wait_for(timeout=30_000)
    log.info("Logged in as '%s'", username)


def xaf_logout(page: Page):
    """Log out of XAF Blazor."""
    log.info("Logging out")
    # Navigate to a known page first to ensure Account button is accessible
    page.goto(BASE_URL)
    page.wait_for_timeout(2000)
    # Open account dropdown menu
    account_btn = page.locator('button[data-action-name="Account"]')
    account_btn.wait_for(timeout=10_000)
    account_btn.click()
    page.wait_for_timeout(500)
    # Click Log Off in the dropdown
    page.click('button[data-action-name="Log Off"]')
    page.locator('input.dxbl-text-edit-input[type="text"]').first.wait_for(timeout=15_000)
    log.info("Logged out")


def navigate_to_dashboards(page: Page):
    """Navigate to SecureDashboardData list view."""
    log.info("Navigating to dashboards")
    page.goto(BASE_URL + "/SecureDashboardData_ListView")
    page.wait_for_timeout(3000)
    page.locator(".dxbl-grid").first.wait_for(timeout=15_000)
    log.info("Dashboard list loaded")


def navigate_to_role_assignments(page: Page):
    """Navigate to DashboardRoleAssignment list view."""
    log.info("Navigating to role assignments")
    page.goto(BASE_URL + "/DashboardRoleAssignment_ListView")
    page.wait_for_timeout(3000)
    page.locator(".dxbl-grid").first.wait_for(timeout=15_000)
    log.info("Role assignment list loaded")


def get_dashboard_titles(page: Page) -> list[str]:
    """Get visible dashboard titles from the dashboard list view."""
    navigate_to_dashboards(page)
    rows = page.locator(".dxbl-grid tr.cursor-pointer").all()
    titles = []
    for row in rows:
        cells = row.locator("td").all()
        # Title is in the 2nd td (index 1); first is checkbox column
        if len(cells) >= 2:
            text = cells[1].text_content().strip()
            if text:
                titles.append(text)
    log.info("Dashboard titles: %s", titles)
    return titles


def get_sidebar_groups(page: Page) -> list[str]:
    """Get visible sidebar navigation group names."""
    texts = page.locator(".xaf-sidebar .dxbl-text").all()
    return [t.text_content().strip() for t in texts if t.is_visible()]
