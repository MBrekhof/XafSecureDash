"""DashboardRoleAssignment management tests."""
import pytest
from playwright.sync_api import Page, expect
import sys
sys.path.insert(0, "..")
from conftest import xaf_login, navigate_to, BASE_URL


@pytest.mark.management
class TestRoleAssignmentManagement:
    def test_admin_can_see_role_assignments(self, page: Page):
        """Admin can navigate to DashboardRoleAssignment list."""
        xaf_login(page, "Admin")
        navigate_to(page, "Administration")
        page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
        page.wait_for_selector(".xaf-listview", timeout=15_000)
        expect(page.locator(".xaf-listview")).to_be_visible()

    def test_admin_can_create_role_assignment(self, page: Page):
        """Admin can create a new DashboardRoleAssignment."""
        xaf_login(page, "Admin")
        navigate_to(page, "Administration")
        page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
        page.wait_for_selector(".xaf-listview", timeout=15_000)

        # Click New button
        page.click('[data-action-id="New"]')
        page.wait_for_selector(".xaf-detailview", timeout=15_000)

        # DetailView should be visible with Dashboard and Role lookups
        expect(page.locator(".xaf-detailview")).to_be_visible()

        # Cancel without saving (we don't want to mess up seed data for other tests)
        page.click('[data-action-id="Cancel"]')

    def test_admin_can_view_existing_assignment(self, page: Page):
        """Admin can click into an existing role assignment detail."""
        xaf_login(page, "Admin")
        navigate_to(page, "Administration")
        page.click('.xaf-navigation >> text="Dashboard Role Assignment"')
        page.wait_for_selector(".xaf-listview", timeout=15_000)

        # Click first row to open detail view
        first_row = page.locator('.xaf-listview [class*="GridRow"]').first
        if first_row.is_visible():
            first_row.click()
            page.wait_for_selector(".xaf-detailview", timeout=15_000)
            expect(page.locator(".xaf-detailview")).to_be_visible()

    def test_user_cannot_access_role_assignments_nav(self, page: Page):
        """Non-admin User should not see Administration nav or DashboardRoleAssignment."""
        xaf_login(page, "User")
        # Administration group should not be in navigation, or DashboardRoleAssignment not listed
        admin_nav = page.locator('.xaf-navigation >> text="Administration"')
        # It might not exist at all, or if it does, DashboardRoleAssignment shouldn't be there
        if admin_nav.is_visible():
            admin_nav.click()
            page.wait_for_timeout(500)
            assignment_nav = page.locator(
                '.xaf-navigation >> text="Dashboard Role Assignment"'
            )
            expect(assignment_nav).not_to_be_visible()
        # If Administration nav doesn't exist, that's also a pass

    def test_manager_cannot_access_role_assignments_nav(self, page: Page):
        """Manager should not see DashboardRoleAssignment in navigation."""
        xaf_login(page, "Manager")
        admin_nav = page.locator('.xaf-navigation >> text="Administration"')
        if admin_nav.is_visible():
            admin_nav.click()
            page.wait_for_timeout(500)
            assignment_nav = page.locator(
                '.xaf-navigation >> text="Dashboard Role Assignment"'
            )
            expect(assignment_nav).not_to_be_visible()
