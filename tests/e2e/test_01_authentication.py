"""Authentication tests for XAF Blazor Server."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, xaf_logout, BASE_URL


@pytest.mark.auth
class TestAuthentication:
    def test_admin_login(self, page: Page):
        """Admin can log in with empty password."""
        xaf_login(page, "Admin")
        expect(page.locator(".xaf-navigation")).to_be_visible()

    def test_user_login(self, page: Page):
        """User can log in with empty password."""
        xaf_login(page, "User")
        expect(page.locator(".xaf-navigation")).to_be_visible()

    def test_manager_login(self, page: Page):
        """Manager can log in with empty password."""
        xaf_login(page, "Manager")
        expect(page.locator(".xaf-navigation")).to_be_visible()

    def test_invalid_password(self, page: Page):
        """Login with wrong password shows error."""
        page.goto(BASE_URL)
        page.wait_for_selector('input[aria-label="User Name"]', timeout=30_000)
        page.fill('input[aria-label="User Name"]', "Admin")
        page.fill('input[aria-label="Password"]', "wrongpassword")
        page.click('div.xaf-logon >> button:has-text("Log In")')
        # XAF shows a validation/error message
        page.wait_for_timeout(2000)
        # Should still be on login page (navigation not visible)
        expect(page.locator('input[aria-label="User Name"]')).to_be_visible()

    def test_logout(self, page: Page):
        """User can log out and is redirected to login page."""
        xaf_login(page, "Admin")
        expect(page.locator(".xaf-navigation")).to_be_visible()
        xaf_logout(page)
        expect(page.locator('input[aria-label="User Name"]')).to_be_visible()
