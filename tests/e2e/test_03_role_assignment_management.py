"""DashboardRoleAssignment management tests."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, navigate_to_role_assignments, get_sidebar_groups, log


@pytest.mark.management
class TestRoleAssignmentManagement:
    def test_admin_can_see_role_assignments(self, page: Page):
        """Admin can navigate to DashboardRoleAssignment list."""
        xaf_login(page, "Admin")
        navigate_to_role_assignments(page)
        expect(page.locator(".dxbl-grid")).to_be_visible()

    def test_admin_can_open_new_form(self, page: Page):
        """Admin can open the New DashboardRoleAssignment form."""
        xaf_login(page, "Admin")
        navigate_to_role_assignments(page)
        # XAF toolbar: look for New button by text content
        new_btn = page.locator('button:has-text("New")').first
        new_btn.click()
        page.wait_for_timeout(3000)
        # The detail view should show form layout fields
        log.info("URL after New: %s", page.url)
        # Verify we're on a form (form layout present)
        expect(page.locator(".dxbl-fl, .dxbl-form-layout, dxbl-form-layout")).to_be_visible()

    def test_admin_can_view_existing_assignment(self, page: Page):
        """Admin can click into an existing role assignment."""
        xaf_login(page, "Admin")
        navigate_to_role_assignments(page)
        first_row = page.locator(".dxbl-grid tr.cursor-pointer").first
        expect(first_row).to_be_visible()
        first_row.click()
        page.wait_for_timeout(3000)
        # Should show a detail/form view
        expect(page.locator(".dxbl-fl, .dxbl-form-layout, dxbl-form-layout")).to_be_visible()
        log.info("Detail view URL: %s", page.url)

    def test_user_cannot_see_role_assignment_nav(self, page: Page):
        """Non-admin User should not see Dashboard Role Assignment in sidebar."""
        xaf_login(page, "User")
        groups = get_sidebar_groups(page)
        log.info("User sidebar: %s", groups)
        assert "Dashboard Role Assignment" not in groups

    def test_manager_cannot_see_role_assignment_nav(self, page: Page):
        """Manager should not see Dashboard Role Assignment in sidebar."""
        xaf_login(page, "Manager")
        groups = get_sidebar_groups(page)
        log.info("Manager sidebar: %s", groups)
        assert "Dashboard Role Assignment" not in groups
