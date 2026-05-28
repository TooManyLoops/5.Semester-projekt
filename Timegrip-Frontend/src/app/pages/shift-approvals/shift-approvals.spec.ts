import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ShiftApprovals } from './shift-approvals';

describe('ShiftApprovals', () => {
  let component: ShiftApprovals;
  let fixture: ComponentFixture<ShiftApprovals>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ShiftApprovals],
    }).compileComponents();

    fixture = TestBed.createComponent(ShiftApprovals);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
