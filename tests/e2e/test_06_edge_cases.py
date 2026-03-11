"""Edge case tests for dashboard security."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, xaf_logout, get_dashboard_titles, BASE_URL, log


@pytest.mark.edge
class TestEdgeCases:
    def test_unauthenticated_redirects_to_login(self, page: Page):
        """Accessing the app without login redirects to login page."""
        page.goto(BASE_URL)
        page.wait_for_timeout(3000)
        expect(page.locator('input.dxbl-text-edit-input[type="text"]').first).to_be_visible()

    def test_public_dashboard_visible_to_all_roles(self, page: Page):
        """Public Overview (no role assignments) is visible to every user."""
        for username in ["Admin", "User", "Manager"]:
            xaf_login(page, username)
            titles = get_dashboard_titles(page)
            assert "Public Overview" in titles, (
                f"{username} should see Public Overview. Got: {titles}"
            )
            xaf_logout(page)

    def test_user_cannot_see_restricted_dashboards(self, page: Page):
        """User should not see dashboards restricted to other roles."""
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "Manager Dashboard" not in titles, f"Got: {titles}"

    def test_admin_always_bypasses_filtering(self, page: Page):
        """Admin sees everything regardless of assignments."""
        xaf_login(page, "Admin")
        titles = get_dashboard_titles(page)
        assert "Public Overview" in titles
        assert "User Dashboard" in titles
        assert "Manager Dashboard" in titles

    def test_multiple_logins_same_browser(self, page: Page):
        """Switching users updates dashboard visibility correctly.

        Note: XAF Blazor uses SignalR circuits. Clearing cookies between
        logins ensures each user gets a fresh circuit and SecuritySystem context.
        """
        xaf_login(page, "User")
        user_titles = get_dashboard_titles(page)
        assert "Manager Dashboard" not in user_titles

        xaf_logout(page)
        # Clear browser state to force a fresh Blazor SignalR circuit
        page.context.clear_cookies()
        xaf_login(page, "Manager")
        manager_titles = get_dashboard_titles(page)
        assert "User Dashboard" not in manager_titles
        assert "Manager Dashboard" in manager_titles

        xaf_logout(page)
        page.context.clear_cookies()
        xaf_login(page, "Admin")
        admin_titles = get_dashboard_titles(page)
        assert len(admin_titles) >= 3
