import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreateShift } from './create-shift';

describe('CreateShift', () => {
  let component: CreateShift;
  let fixture: ComponentFixture<CreateShift>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [CreateShift],
    }).compileComponents();

    fixture = TestBed.createComponent(CreateShift);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
