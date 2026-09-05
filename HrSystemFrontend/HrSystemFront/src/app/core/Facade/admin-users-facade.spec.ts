import { TestBed } from '@angular/core/testing';
import { AdminUsersFacade } from './admin-users-facade';

describe('AdminUsersFacade', () => {
  let service: AdminUsersFacade;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AdminUsersFacade);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
