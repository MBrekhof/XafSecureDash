"""Dashboard visibility tests — core POC validation."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, get_dashboard_titles, BASE_URL


@pytest.mark.visibility
class TestDashboardVisibility:
    def test_admin_sees_all_dashboards(self, page: Page):
        """Admin (IsAdministrative) should see all 3 dashboards."""
        xaf_login(page, "Admin")
        titles = get_dashboard_titles(page)
        assert "Public Overview" in titles
        assert "User Dashboard" in titles
        assert "Manager Dashboard" in titles
        assert len(titles) >= 3

    def test_user_sees_public_and_user_dashboard(self, page: Page):
        """User (Default role) should see Public Overview and User Dashboard."""
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "Public Overview" in titles
        assert "User Dashboard" in titles

    def test_user_does_not_see_manager_dashboard(self, page: Page):
        """User (Default role) should NOT see Manager Dashboard."""
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert "Manager Dashboard" not in titles

    def test_manager_sees_public_and_manager_dashboard(self, page: Page):
        """Manager should see Public Overview and Manager Dashboard."""
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "Public Overview" in titles
        assert "Manager Dashboard" in titles

    def test_manager_does_not_see_user_dashboard(self, page: Page):
        """Manager should NOT see User Dashboard."""
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert "User Dashboard" not in titles

    def test_user_exact_dashboard_count(self, page: Page):
        """User should see exactly 2 dashboards."""
        xaf_login(page, "User")
        titles = get_dashboard_titles(page)
        assert len(titles) == 2, f"Expected 2 dashboards, got {len(titles)}: {titles}"

    def test_manager_exact_dashboard_count(self, page: Page):
        """Manager should see exactly 2 dashboards."""
        xaf_login(page, "Manager")
        titles = get_dashboard_titles(page)
        assert len(titles) == 2, f"Expected 2 dashboards, got {len(titles)}: {titles}"
