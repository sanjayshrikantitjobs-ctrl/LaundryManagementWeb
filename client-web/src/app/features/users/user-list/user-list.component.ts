import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { UsersService } from '../users.service';
import { STAFF_ROLES, UserRole, UserSummary } from '../../../core/models/user.models';
import { SortDirection } from '../../../core/models/order.models';
import { PaginationComponent } from '../../../shared/pagination/pagination.component';
import { ConfirmDialogService } from '../../../shared/confirm-dialog/confirm-dialog.service';

type UserTab = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule, RouterLink, PaginationComponent],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss'
})
export class UserListComponent implements OnInit {
  private readonly confirmDialog = inject(ConfirmDialogService);

  readonly users = signal<UserSummary[]>([]);
  readonly isLoading = signal(true);
  readonly search = signal('');
  readonly totalCount = signal(0);
  readonly activeTab = signal<UserTab>('all');
  readonly roleFilter = signal<UserRole | null>(null);
  readonly UserRole = UserRole;
  readonly staffRoles = STAFF_ROLES;

  readonly pageNumber = signal(1);
  readonly pageSize = signal(20);
  readonly sortBy = signal<string | null>(null);
  readonly sortDirection = signal<SortDirection>('asc');

  constructor(private usersService: UsersService) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  onSearch(term: string): void {
    this.search.set(term);
    this.pageNumber.set(1);
    this.loadUsers();
  }

  setTab(tab: UserTab): void {
    this.activeTab.set(tab);
    this.pageNumber.set(1);
    this.loadUsers();
  }

  onRoleFilterChange(value: string): void {
    this.roleFilter.set(value === '' ? null : (Number(value) as UserRole));
    this.pageNumber.set(1);
    this.loadUsers();
  }

  onPageChange(page: number): void {
    this.pageNumber.set(page);
    this.loadUsers();
  }

  onPageSizeChange(size: number): void {
    this.pageSize.set(size);
    this.pageNumber.set(1);
    this.loadUsers();
  }

  sortByColumn(column: string): void {
    if (this.sortBy() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortBy.set(column);
      this.sortDirection.set('asc');
    }
    this.pageNumber.set(1);
    this.loadUsers();
  }

  sortIndicator(column: string): string {
    if (this.sortBy() !== column) return '';
    return this.sortDirection() === 'asc' ? '▲' : '▼';
  }

  roleLabel(role: UserRole): string {
    return UserRole[role];
  }

  async toggleActive(user: UserSummary): Promise<void> {
    const action = user.isActive ? 'Deactivate' : 'Activate';
    const result = await this.confirmDialog.confirm({
      title: `${action} user`,
      message: `${action} user "${user.fullName}"?`,
      confirmLabel: action
    });
    if (!result.confirmed) return;

    this.usersService.setActive(user.id, !user.isActive).subscribe({
      next: () => this.loadUsers()
    });
  }

  private loadUsers(): void {
    this.isLoading.set(true);
    const tab = this.activeTab();
    this.usersService
      .getUsers({
        search: this.search() || undefined,
        role: this.roleFilter() ?? undefined,
        isActive: tab === 'active' ? true : tab === 'inactive' ? false : undefined,
        pageNumber: this.pageNumber(),
        pageSize: this.pageSize(),
        sortBy: this.sortBy() ?? undefined,
        sortDirection: this.sortDirection()
      })
      .subscribe({
        next: (result) => {
          this.users.set(result.items);
          this.totalCount.set(result.totalCount);
          this.isLoading.set(false);
        },
        error: () => this.isLoading.set(false)
      });
  }
}
