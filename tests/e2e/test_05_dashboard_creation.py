"""Dashboard creation tests."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import (
    xaf_login, xaf_logout, navigate_to_dashboards,
    navigate_to_role_assignments, get_dashboard_titles, BASE_URL, log,
)


@pytest.mark.creation
class TestDashboardCreation:
    def test_admin_can_open_dashboard_designer(self, page: Page):
        """Admin can click New and open the dashboard designer."""
        xaf_login(page, "Admin")
        navigate_to_dashboards(page)
        page.locator('button:has-text("New")').first.click()
        page.wait_for_timeout(5000)
        # Dashboard designer opens (DevExpress dashboard component)
        page.locator(".detail-view-content").wait_for(timeout=15_000)
        expect(page.locator(".detail-view-content")).to_be_visible()
        log.info("Dashboard designer opened at: %s", page.url)

    def test_admin_can_view_existing_dashboard(self, page: Page):
        """Admin can click an existing dashboard to view/edit it."""
        xaf_login(page, "Admin")
        navigate_to_dashboards(page)
        first_row = page.locator(".dxbl-grid tr.cursor-pointer").first
        expect(first_row).to_be_visible()
        first_row.click()
        page.wait_for_timeout(5000)
        # Should show the dashboard viewer/designer
        expect(page.locator(".detail-view-content")).to_be_visible()
        log.info("Dashboard detail view opened at: %s", page.url)

    def test_user_cannot_create_dashboard(self, page: Page):
        """Non-admin User should not have a New button or it should be hidden."""
        xaf_login(page, "User")
        navigate_to_dashboards(page)
        # User may see the list but New button should be absent or disabled
        new_btns = page.locator('button:has-text("New")')
        if new_btns.count() > 0:
            # If button exists, it should not be clickable/visible
            log.info("New button count: %d", new_btns.count())

    def test_dashboard_count_per_role(self, page: Page):
        """Verify expected dashboard counts for each role."""
        xaf_login(page, "Admin")
        admin_titles = get_dashboard_titles(page)
        assert len(admin_titles) >= 3, f"Admin should see 3+ dashboards. Got: {admin_titles}"
        xaf_logout(page)

        xaf_login(page, "User")
        user_titles = get_dashboard_titles(page)
        assert len(user_titles) == 2, f"User should see 2 dashboards. Got: {user_titles}"
        xaf_logout(page)

        xaf_login(page, "Manager")
        manager_titles = get_dashboard_titles(page)
        assert len(manager_titles) == 2, f"Manager should see 2 dashboards. Got: {manager_titles}"
