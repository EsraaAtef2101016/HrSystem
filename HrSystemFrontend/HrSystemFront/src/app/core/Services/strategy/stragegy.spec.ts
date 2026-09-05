import { TestBed } from '@angular/core/testing';
import { Stragegy } from './stragegy';

describe('Stragegy', () => {
  let service: Stragegy;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Stragegy);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
